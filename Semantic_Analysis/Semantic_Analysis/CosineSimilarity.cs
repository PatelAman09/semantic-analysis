using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                using (var reader = new StreamReader(inputFilePath))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        // Split the line by commas and parse values to double
                        var values = line.Split(',');

                        //var vector = Array.ConvertAll(values, double.Parse);
                        //vectors.Add(vector);
                        // Skip lines that don't contain valid numeric data
                        var vector = new double[values.Length];
                        bool validLine = true;

                        for (int i = 0; i < values.Length; i++)
                        {
                            if (!double.TryParse(values[i], out vector[i]))
                            {
                                validLine = false;
                                break;
                            }
                        }

                        if (validLine)
                        {
                            vectors.Add(vector); // Only add valid vectors
                        }
                        else
                        {
                            Console.WriteLine($"Skipping invalid line: {line}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading CSV file: {ex.Message}");
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
        }
        // Function to calculate the dot product of two vectors
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
            string inputFilePath = "E:\\Test\\sample_vectors.csv"; // Provide the path to your input CSV file
            string outputFilePath = "E:\\Test\\output_file.csv"; // Provide the path to save the output

            try
            {
                // Step 1: Read vectors from the CSV file
                List<double[]> vectors = CosineSimilarity.ReadVectorsFromCsv(inputFilePath);

                // Step 2: Validate that there are at least two vectors
                CosineSimilarity.ValidateVectors(vectors);

                // Step 3: Calculate the cosine similarity for each pair of vectors
                List<string> outputData = new List<string>
                {
                    "Vector1,Vector2,CosineSimilarity" // Header row for the output CSV
                };

                for (int i = 0; i < vectors.Count - 1; i++)
                {
                    for (int j = i + 1; j < vectors.Count; j++)
                    {
                        double similarity = CosineSimilarity.CosineSimilarityCalculation(vectors[i], vectors[j]);
                        outputData.Add($"{i + 1},{j + 1},{similarity:F4}"); // Add the similarity for this pair
                    }
                }

                // Step 4: Save the results to a CSV file
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
            using (var writer = new StreamWriter(filePath))
            {
                foreach (string line in data)
                {
                    writer.WriteLine(line);
                }
            }
        }
    }
}
