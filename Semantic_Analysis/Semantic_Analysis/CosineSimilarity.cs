using Microsoft.Extensions.Configuration;
using Semantic_Analysis.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semantic_Analysis
{
    public class CosineSimilarity : ICosineSimilarity
    {
        public Dictionary<string, (string text, double[] vector)> ReadVectorsFromCsv(string inputFilePath)
        {
            if (string.IsNullOrEmpty(inputFilePath))
                throw new ArgumentException("File path must not be null or empty");

            var vectors = new Dictionary<string, (string text, double[] vector)>();

            try
            {
                foreach (var line in File.ReadLines(inputFilePath, Encoding.UTF8))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split(',');

                    if (parts.Length < 2) continue; // Skip malformed lines

                    // Extract index and text
                    string rawText = parts[0].Trim().Trim('"');
                    string index = rawText.Contains(":") ? rawText.Split(':')[0].Trim() : "[Unknown]";
                    string cleanedText = rawText.Contains(":") ? rawText.Split(':').Last().Trim() : rawText;

                    // Parse vector values safely
                    List<double> vectorValues = new List<double>();
                    foreach (var value in parts.Skip(1))
                    {
                        string cleanedValue = value.Trim().Trim('"'); // Remove extra quotes
                        if (double.TryParse(cleanedValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double num))
                        {
                            vectorValues.Add(num);
                        }
                        else
                        {
                            Console.WriteLine($"Warning: Invalid number format in CSV: {value}");
                        }
                    }

                    if (!string.IsNullOrEmpty(cleanedText) && vectorValues.Count > 0)
                        vectors[index] = (cleanedText, NormalizeVector(vectorValues.ToArray()));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading CSV file '{inputFilePath}': {ex.Message}");
                throw;
            }

            if (vectors.Count < 1)
                throw new InvalidOperationException("CSV must contain at least one valid vector.");

            return vectors;
        }

        public double[] NormalizeVector(double[] vector)
        {
            double magnitude = Math.Sqrt(vector.Sum(v => v * v));
            return magnitude == 0 ? vector : vector.Select(v => v / magnitude).ToArray();
        }

        public void ValidateVectors(Dictionary<string, (string text, double[] vector)> vectors)
        {
            if (vectors.Count < 1)
                throw new InvalidOperationException("Each file must contain at least one valid vector.");

            int vectorLength = vectors.Values.First().vector.Length;
            if (vectors.Values.Any(v => v.vector.Length != vectorLength))
                throw new InvalidOperationException("All vectors must have the same length.");
        }

        public double CosineSimilarityCalculation(double[] vectorA, double[] vectorB)
        {
            double dotProduct = vectorA.Zip(vectorB, (a, b) => a * b).Sum();
            double magnitudeA = Math.Sqrt(vectorA.Sum(v => v * v));
            double magnitudeB = Math.Sqrt(vectorB.Sum(v => v * v));

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
            string inputFile1 = config["FilePaths:InputFileName1"];
            string inputFile2 = config["FilePaths:InputFileName2"];
            string outputFileName = config["FilePaths:CSVOutputFileName"];

            string rootDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            string inputFilePath1 = Path.Combine(rootDirectory, config["FilePaths:InputFolder"], inputFile1);
            string inputFilePath2 = Path.Combine(rootDirectory, config["FilePaths:InputFolder"], inputFile2);
            string outputFilePath = Path.Combine(rootDirectory, config["FilePaths:OutputFolder"], outputFileName);

            ICosineSimilarity cosineSimilarity = new CosineSimilarity();

            try
            {
                Dictionary<string, (string text, double[] vector)> vectorsFile1 = cosineSimilarity.ReadVectorsFromCsv(inputFilePath1);
                Dictionary<string, (string text, double[] vector)> vectorsFile2 = cosineSimilarity.ReadVectorsFromCsv(inputFilePath2);

                if (vectorsFile1.Count == 0 || vectorsFile2.Count == 0)
                {
                    throw new InvalidOperationException("Each input file must contain at least one valid vector.");
                }

                cosineSimilarity.ValidateVectors(vectorsFile1);
                cosineSimilarity.ValidateVectors(vectorsFile2);

                ConcurrentBag<string> outputData = new ConcurrentBag<string>();
                outputData.Add("Index1,Index2,Word1,Word2,X-Position,Cosine Similarity");

                int totalWords = vectorsFile1.Count;

                Parallel.ForEach(vectorsFile1.Keys, (index1, _, idx) =>
                {
                    foreach (var index2 in vectorsFile2.Keys)
                    {
                        double similarity = Math.Round(
                            cosineSimilarity.CosineSimilarityCalculation(vectorsFile1[index1].vector, vectorsFile2[index2].vector),
                            10
                        );

                        int xPosition = (int)(((double)idx / totalWords) * 536.0); // Scaling factor

                        outputData.Add($"{index1},{index2},\"{vectorsFile1[index1].text}\",\"{vectorsFile2[index2].text}\",{xPosition},{similarity.ToString(CultureInfo.InvariantCulture)}");
                    }
                });

                cosineSimilarity.SaveOutputToCsv(outputFilePath, outputData.ToList());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}