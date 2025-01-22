using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Semantic_Analysis
{
    public class CosineSimilarity
    {
        public static List<double[]> ReadVectorsFromCsv(string inputFilePath)
        {
            if (string.IsNullOrEmpty(inputFilePath))
                throw new ArgumentException("File path must not be null or empty");

            var vectors = new List<double[]>();
            try
            {
                using (var reader = new StreamReader(inputFilePath, Encoding.UTF8, true, 65536))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        // Process the line to extract vector data
                        var parts = line.Split(new[] { ':' }, 2); // Split at first colon to separate label
                        if (parts.Length < 2) continue; // If no vector data, skip

                        // Extract the numeric part after the colon
                        var vectorPart = parts[1];

                        // Clean up any non-numeric identifiers or metadata before the actual vector
                        var vectorData = vectorPart.Split(',').Skip(1).FirstOrDefault(); // Skip metadata like IDs

                        if (!string.IsNullOrEmpty(vectorData))
                        {
                            // Split the vector by commas to get individual values and parse them into a double array
                            var vectorValues = vectorData.Split(',')
                                                         .Select(value => double.Parse(value.Trim('"')))
                                                         .ToArray();

                            vectors.Add(vectorValues);
                            Console.WriteLine($"Added vector: {string.Join(",", vectorValues)}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading CSV file: {ex.Message}");
            }

            if (vectors.Count < 2)
            {
                Console.WriteLine("CSV must contain at least two vectors.");
                throw new InvalidOperationException("Insufficient number of vectors for cosine similarity calculation.");
            }

            return vectors;
        }

        public static void ValidateVectors(List<double[]> vectors)
        {
            if (vectors.Count < 2)
            {
                Console.WriteLine("CSV must contain at least two vectors.");
                throw new InvalidOperationException("Insufficient number of vectors for cosine similarity calculation.");
            }

            int vectorLength = vectors[0].Length;
            foreach (var vector in vectors)
            {
                if (vector.Length != vectorLength)
                {
                    throw new InvalidOperationException("All vectors must be of the same length.");
                }
            }
        }

        public static double CalculateDotProduct(double[] vectorA, double[] vectorB)
        {
            if (vectorA.Length != vectorB.Length)
                throw new ArgumentException("Vectors must be of the same length");

            double dotProduct = 0.0;
            for (int i = 0; i < vectorA.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
            }
            return dotProduct;
        }

        // Function to calculate the magnitude of a vector
        public static double CalculateMagnitude(double[] vector)
        {
            if (vector == null || vector.Length == 0)
                throw new ArgumentException("Vector must not be null or empty");

            double sumOfSquares = 0.0;
            for (int i = 0; i < vector.Length; i++)
            {
                sumOfSquares += vector[i] * vector[i];
            }

            return Math.Sqrt(sumOfSquares);
        }

        // Function to calculate cosine similarity
        public static double CosineSimilarityCalculation(double[] vectorA, double[] vectorB)
        {
            if (vectorA == null || vectorB == null)
                throw new ArgumentException("Vectors must not be null");

            double dotProduct = CalculateDotProduct(vectorA, vectorB);
            double magnitudeA = CalculateMagnitude(vectorA);
            double magnitudeB = CalculateMagnitude(vectorB);

            // Avoid division by zero
            if (magnitudeA == 0 || magnitudeB == 0)
                return 0.0;

            return dotProduct / (magnitudeA * magnitudeB);
        }

        public static void Main(string[] args)
        {
            string inputFilePath = "E:\\Test\\input.csv"; // You can modify this for user input
            string outputFilePath = "E:\\Test\\output.csv";

            try
            {
                List<double[]> vectors = ReadVectorsFromCsv(inputFilePath);
                ValidateVectors(vectors);

                List<string> outputData = new List<string>
                {
                    "CosineSimilarity" // Header
                };

                for (int i = 0; i < vectors.Count - 1; i++)
                {
                    for (int j = i + 1; j < vectors.Count; j++)
                    {
                        double similarity = CosineSimilarityCalculation(vectors[i], vectors[j]);
                        outputData.Add($"{similarity}");
                    }
                }

                SaveOutputToCsv(outputFilePath, outputData);

                Console.WriteLine("Cosine similarity calculations saved to the output CSV file.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        // Function to save the output data to a CSV file
        public static void SaveOutputToCsv(string filePath, List<string> data)
        {
            try
            {
                using (var writer = new StreamWriter(filePath))
                {
                    foreach (string line in data)
                    {
                        writer.WriteLine(line);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while saving the output: {ex.Message}");
            }
        }
    }
}