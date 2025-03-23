
namespace Semantic_Analysis.Interfaces
{
    /// <summary>
    /// Interface for calculating and handling cosine similarity between document vectors.
    /// </summary>
    public interface ICosineSimilarity
    {
        /// <summary>
        /// Reads vectors from a CSV file and stores them in a dictionary.
        /// </summary>
        /// <param name="inputFilePath">The path of the CSV file to read vectors from.</param>
        /// <returns>A dictionary where the key is the index and the value is a tuple containing the text and the vector array.</returns>
        /// <exception cref="ArgumentException">Thrown if the input file path is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the CSV file contains no valid vectors.</exception>
        Dictionary<string, (string text, double[] vector)> ReadVectorsFromCsv(string inputFilePath);

        /// <summary>
        /// Normalizes a vector by dividing each element by its magnitude.
        /// </summary>
        /// <param name="vector">The vector to normalize.</param>
        /// <returns>The normalized vector.</returns>
        double[] NormalizeVector(double[] vector);

        /// <summary>
        /// Validates that all vectors in the given dictionary are of the same length.
        /// </summary>
        /// <param name="vectors">The dictionary of vectors to validate.</param>
        /// <exception cref="InvalidOperationException">Thrown if vectors are not of the same length.</exception>
        void ValidateVectors(Dictionary<string, (string text, double[] vector)> vectors);

        /// <summary>
        /// Calculates the cosine similarity between two vectors.
        /// </summary>
        /// <param name="vectorA">The first vector.</param>
        /// <param name="vectorB">The second vector.</param>
        /// <returns>The cosine similarity between the two vectors, a value between -1 and 1.</returns>
        double CosineSimilarityCalculation(double[] vectorA, double[] vectorB);

        /// <summary>
        /// Saves the output data to a CSV file.
        /// </summary>
        /// <param name="outputFilePath">The path of the output file.</param>
        /// <param name="outputData">The data to write to the CSV file.</param>
        void SaveOutputToCsv(string outputFilePath, List<string> outputData);

        /// <summary>
        /// Calculates the average cosine similarity between vectors from two files.
        /// </summary>
        /// <param name="vectorsFile1">The vectors from the first file.</param>
        /// <param name="vectorsFile2">The vectors from the second file.</param>
        /// <returns>The average cosine similarity between all vector pairs.</returns>
        double CalculateDocumentSimilarity(Dictionary<string, (string text, double[] vector)> vectorsFile1, Dictionary<string, (string text, double[] vector)> vectorsFile2);
    }
}

