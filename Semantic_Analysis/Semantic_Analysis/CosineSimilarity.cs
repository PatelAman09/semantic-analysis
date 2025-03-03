using Microsoft.Extensions.Configuration;
using Semantic_Analysis.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

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

                    var parts = line.Split(',');
                    if (parts.Length < 2) continue; // Skip malformed lines

                    string rawWord = parts[0].Trim('\"');
                    string cleanedWord = rawWord.Contains(":") ? rawWord.Split(':').Last().Trim() : rawWord; // Clean word

                    double[] vectorValues = parts.Skip(1)
                                                 .Select(value => double.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double num) ? num : double.NaN)
                                                 .Where(num => !double.IsNaN(num))
                                                 .ToArray();

                    if (!string.IsNullOrEmpty(cleanedWord) && vectorValues.Length > 0)
                        vectors[cleanedWord] = NormalizeVector(vectorValues);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading CSV file '{inputFilePath}': {ex.Message}");
                throw;
            }

            if (vectors.Count < 2)
                throw new InvalidOperationException("CSV must contain at least two valid vectors.");

            return vectors;
        }

        public double[] NormalizeVector(double[] vector)
        {
            double magnitude = Math.Sqrt(vector.Sum(v => v * v));
            return magnitude == 0 ? vector : vector.Select(v => v / magnitude).ToArray();
        }

        public void ValidateVectors(Dictionary<string, double[]> vectors)
        {
            if (vectors.Count < 2)
                throw new InvalidOperationException("Insufficient number of vectors for cosine similarity calculation.");

            int vectorLength = vectors.Values.First().Length;
            if (vectors.Values.Any(v => v.Length != vectorLength))
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
                Dictionary<string, double[]> vectorsFile1 = cosineSimilarity.ReadVectorsFromCsv(inputFilePath1);
                Dictionary<string, double[]> vectorsFile2 = cosineSimilarity.ReadVectorsFromCsv(inputFilePath2);

                cosineSimilarity.ValidateVectors(vectorsFile1);
                cosineSimilarity.ValidateVectors(vectorsFile2);

                ConcurrentBag<string> outputData = new ConcurrentBag<string>();
                outputData.Add("Word1,Word2,X-Position,Cosine Similarity");

                int totalWords = vectorsFile1.Count;

                Parallel.ForEach(vectorsFile1.Keys, (file1Key, _, index) =>
                {
                    foreach (var file2Key in vectorsFile2.Keys)
                    {
                        double similarity = Math.Round(cosineSimilarity.CosineSimilarityCalculation(vectorsFile1[file1Key], vectorsFile2[file2Key]), 10);

                        int xPosition = (int)(((double)index / totalWords) * 536.0); // Ensure proper scaling
                        outputData.Add($"{file1Key},{file2Key},{xPosition},{similarity.ToString(CultureInfo.InvariantCulture)}");
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
