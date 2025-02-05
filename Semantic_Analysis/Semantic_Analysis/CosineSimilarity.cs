using Microsoft.Extensions.Configuration;
using Semantic_Analysis.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semantic_Analysis
{
    public class CosineSimilarity : ICosineSimilarity
    {
        public List<double[]> ReadVectorsFromCsv(string inputFilePath)
        {
            if (string.IsNullOrEmpty(inputFilePath))
                throw new ArgumentException("File path must not be null or empty");

            var vectors = new List<double[]>();

            try
            {
                foreach (var line in File.ReadLines(inputFilePath, Encoding.UTF8))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split(new[] { ':' }, 2);
                    if (parts.Length < 2) continue; // Skip lines without a vector

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
                        vectors.Add(vectorValues.ToArray());
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

        public void ValidateVectors(List<double[]> vectors)
        {
            if (vectors.Count < 2)
                throw new InvalidOperationException("Insufficient number of vectors for cosine similarity calculation.");

            int vectorLength = vectors[0].Length;
            if (vectors.Any(v => v.Length != vectorLength))
                throw new InvalidOperationException("All vectors must have the same length.");
        }

        public double CalculateDotProduct(double[] vectorA, double[] vectorB)
        {
            if (vectorA.Length != vectorB.Length)
                throw new ArgumentException("Vectors must be of the same length");

            return vectorA.Zip(vectorB, (a, b) => a * b).Sum();
        }

        public double CalculateMagnitude(double[] vector)
        {
            return Math.Sqrt(vector.Sum(v => v * v));
        }

        public double CosineSimilarityCalculation(double[] vectorA, double[] vectorB)
        {
            double dotProduct = CalculateDotProduct(vectorA, vectorB);
            double magnitudeA = CalculateMagnitude(vectorA);
            double magnitudeB = CalculateMagnitude(vectorB);

            if (magnitudeA == 0 || magnitudeB == 0)
                return 0.0;

            return dotProduct / (magnitudeA * magnitudeB);
        }

        public void SaveOutputToCsv(string outputFilePath, List<string> data)
        {
            try
            {
                File.WriteAllLines(outputFilePath, data);
                Console.WriteLine($"Results saved to {outputFilePath}");
            }
            catch (Exception ex)
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

        //public static void Main(string[] args)
        //{
        //    // Load configuration
        //    IConfigurationRoot config = LoadConfiguration();
        //    string inputFolder = config["FilePaths:InputFolder"];
        //    string outputFolder = config["FilePaths:OutputFolder"];
        //    string inputFileName = config["FilePaths:InputFileName"];  // Read input file name from config
        //    string outputFileName = config["FilePaths:CSVOutputFileName"];  // Read output file name from config

        //    // Get the root directory (where your project files are located)
        //    string rootDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;

        //    // Combine the root directory with the input folder and file name
        //    string inputFilePath = Path.Combine(rootDirectory, inputFolder, inputFileName);
        //    string outputFilePath = Path.Combine(rootDirectory, outputFolder, outputFileName);

        //    // Null checks to avoid possible null reference warnings
        //    if (string.IsNullOrEmpty(inputFolder) || string.IsNullOrEmpty(outputFolder) ||
        //        string.IsNullOrEmpty(inputFileName) || string.IsNullOrEmpty(outputFileName))
        //    {
        //        throw new InvalidOperationException("One or more configuration values are missing or invalid in appsettings.json.");
        //    }

        //    ICosineSimilarity cosineSimilarity = new CosineSimilarity();

        //    try
        //    {
        //        // Read and validate vectors
        //        List<double[]> vectors = cosineSimilarity.ReadVectorsFromCsv(inputFilePath);
        //        cosineSimilarity.ValidateVectors(vectors);

        //        List<string> outputData = new List<string> { "CosineSimilarity" };

        //        Parallel.For(0, vectors.Count - 1, i =>
        //        {
        //            for (int j = i + 1; j < vectors.Count; j++)
        //            {
        //                double similarity = Math.Round(cosineSimilarity.CosineSimilarityCalculation(vectors[i], vectors[j]), 4);
        //                string result = $"Vector {i} vs Vector {j}: {similarity}";
        //                Console.WriteLine(result);
        //                outputData.Add(result);
        //            }
        //        });

        //        // Save the output
        //        cosineSimilarity.SaveOutputToCsv(outputFilePath, outputData);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"An error occurred: {ex.Message}");
        //    }
        //}
    }
}
