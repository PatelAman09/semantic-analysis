using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semantic_Analysis
{
    public class CosineSimilarity
    {
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

    }
}
