using Microsoft.Extensions.Configuration;
using Semantic_Analysis.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semantic_Analysis
{
    public class CosineSimilarity : ICosineSimilarity
    {
        public Dictionary<string, double[]> ReadVectorsFromCsv(string inputFilePath)
        {
            if (string.IsNullOrEmpty(inputFilePath))
                throw new ArgumentException("File path must not be null or empty");

            var vectors = new Dictionary<string, double[]>();

            try
            {
                foreach (var line in File.ReadLines(inputFilePath, Encoding.UTF8))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split(new[] { ':' }, 2);
                    if (parts.Length < 2) continue; // Skip invalid lines

                    string vectorName = parts[0].Trim(new[] { '[', ']', ' ' });
                    var vectorPart = parts[1].Trim();
                    var values = vectorPart.Split(',');

                    var vectorValues = new List<double>();
                    foreach (var value in values)
                    {
                        if (double.TryParse(value.Trim('"'), out double number))
                        {
                            vectorValues.Add(number);
                        }
                        else
                        {
                            Console.WriteLine($"Warning: Skipped invalid value '{value}' in line: {line}");
                        }
                    }

                    if (vectorValues.Count > 0)
                    {
                        vectors[vectorName] = vectorValues.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading CSV file '{inputFilePath}': {ex.Message}");
            }

            if (vectors.Count < 2)
            {
                throw new InvalidOperationException("CSV must contain at least two valid vectors.");
            }

            return vectors;
        }

        public void ValidateVectors(Dictionary<string, double[]> vectors)
        {
            if (vectors.Count < 2)
                throw new InvalidOperationException("Insufficient number of vectors for cosine similarity calculation.");

            int vectorLength = vectors.Values.First().Length;
            if (vectors.Values.Any(v => v.Length != vectorLength))
                throw new InvalidOperationException("All vectors must have the same length.");
        }

        public double CalculateDotProduct(double[] vectorA, double[] vectorB)
        {
            double sum = 0;
            for (int i = 0; i < vectorA.Length; i++)
            {
                sum += vectorA[i] * vectorB[i];
            }
            return sum;
        }

        public double CalculateMagnitude(double[] vector)
        {
            double sum = vector.Sum(v => v * v);
            return sum > 0 ? Math.Sqrt(sum) : 0;
        }
        public double CosineSimilarityCalculation(double[] vectorA, double[] vectorB)
        {
            double dotProduct = CalculateDotProduct(vectorA, vectorB);
            double magnitudeA = CalculateMagnitude(vectorA);
            double magnitudeB = CalculateMagnitude(vectorB);

            return (magnitudeA == 0 || magnitudeB == 0) ? 0.0 : dotProduct / (magnitudeA * magnitudeB);
        }

        public void SaveOutputToCsv(string outputFilePath, List<string> outputData)
        {
            try
            {
                File.WriteAllLines(outputFilePath, outputData);
                Console.WriteLine($"Results saved to {outputFilePath}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error saving output file '{outputFilePath}': {ex.Message}");
            }
        }

        public static IConfigurationRoot LoadConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        public static void Main(string[] args)
        {
            IConfigurationRoot config = LoadConfiguration();
            string inputFolder = config["FilePaths:InputFolder"];
            string outputFolder = config["FilePaths:OutputFolder"];
            string inputFile1 = config["FilePaths:InputFileName1"];
            string inputFile2 = config["FilePaths:InputFileName2"];
            string outputFileName = config["FilePaths:CSVOutputFileName"];

            string rootDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;

            string inputFilePath1 = Path.Combine(rootDirectory, inputFolder, inputFile1);
            string inputFilePath2 = Path.Combine(rootDirectory, inputFolder, inputFile2);
            string outputFilePath = Path.Combine(rootDirectory, outputFolder, outputFileName);

            if (string.IsNullOrEmpty(inputFile1) || string.IsNullOrEmpty(inputFile2) || string.IsNullOrEmpty(outputFileName))
                throw new InvalidOperationException("Missing configuration values in appsettings.json.");

            ICosineSimilarity cosineSimilarity = new CosineSimilarity();

            try
            {
                // Read vectors from both CSV files
                Dictionary<string, double[]> vectorsFile1 = cosineSimilarity.ReadVectorsFromCsv(inputFilePath1);
                Dictionary<string, double[]> vectorsFile2 = cosineSimilarity.ReadVectorsFromCsv(inputFilePath2);

                // Validate both vector dictionaries
                cosineSimilarity.ValidateVectors(vectorsFile1);
                cosineSimilarity.ValidateVectors(vectorsFile2);

                if (vectorsFile1.Values.First().Length != vectorsFile2.Values.First().Length)
                {
                    throw new InvalidOperationException("Vectors from both files must have the same dimensions.");
                }

                // Initialize List to collect output data
                List<string> outputData = new List<string>();
                outputData.Add("Cosine Similarity Results");

                // Compute similarity between each vector from File1 and each vector from File2
                object lockObject = new object();

                List<string> outputData = new List<string> { "CosineSimilarity" };

                Parallel.ForEach(vectorsFile1.Keys, file1Key =>
                {
                    foreach (var file2Key in vectorsFile2.Keys)
                    {
                        // Calculate the cosine similarity between the two vectors
                        double similarity = Math.Round(cosineSimilarity.CosineSimilarityCalculation(vectorsFile1[file1Key], vectorsFile2[file2Key]), 10);

                        // Create the result string with the correct names (instead of indices)
                        string result = $"\"{file1Key}\" (File1) vs \"{file2Key}\" (File2): {similarity}";

                        // Add the result to the output list
                        outputData.Add(result);
                    }
                });

                // Save results
                cosineSimilarity.SaveOutputToCsv(outputFilePath, outputData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
