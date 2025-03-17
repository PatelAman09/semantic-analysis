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

                    var parts = line.Split(','); // Split normally first
                    if (parts.Length < 2) continue; // Skip malformed lines

                    // Extract index and text
                    string rawText = parts[0].Trim().Trim('"');
                    string index = rawText.Contains(":") ? rawText.Split(':')[0].Trim() : "[Unknown]";
                    string cleanedText = rawText.Contains(":") ? rawText.Substring(rawText.IndexOf(':') + 1).Trim() : rawText;

                    // **Fix for cases where text contains commas**
                    int lastTextIndex = 0;
                    for (int i = 1; i < parts.Length; i++)
                    {
                        if (!double.TryParse(parts[i].Trim().Trim('"'), NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                        {
                            cleanedText += "," + parts[i].Trim().Trim('"');
                            lastTextIndex = i;
                        }
                        else
                        {
                            break;
                        }
                    }

                    // Extract vector values safely after the complete text
                    List<double> vectorValues = new List<double>();
                    for (int i = lastTextIndex + 1; i < parts.Length; i++)
                    {
                        string cleanedValue = parts[i].Trim().Trim('"');
                        if (double.TryParse(cleanedValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double num))
                        {
                            vectorValues.Add(num);
                        }
                        else
                        {
                            Console.WriteLine($"Warning: Invalid number format in CSV: {parts[i]}");
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
            // Cosine similarity formula: dot product / (magnitude of A * magnitude of B)
            double dotProduct = vectorA.Zip(vectorB, (a, b) => a * b).Sum();
            double magnitudeA = Math.Sqrt(vectorA.Sum(v => v * v));
            double magnitudeB = Math.Sqrt(vectorB.Sum(v => v * v));

            // This will return a value between -1 and 1
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

        public double CalculateDocumentSimilarity(Dictionary<string, (string text, double[] vector)> vectorsFile1,
                                          Dictionary<string, (string text, double[] vector)> vectorsFile2)
        {
            double totalSimilarity = 0;
            int comparisonCount = 0;

            foreach (var entry1 in vectorsFile1)
            {
                foreach (var entry2 in vectorsFile2)
                {
                    double similarity = CosineSimilarityCalculation(entry1.Value.vector, entry2.Value.vector);
                    totalSimilarity += similarity;
                    comparisonCount++;
                }
            }

            return comparisonCount > 0 ? totalSimilarity / comparisonCount : 0;
        }
    }
}