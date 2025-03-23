
namespace Semantic_Analysis.Interfaces
{
    /// <summary>
    /// Interface defining methods for processing embeddings and interacting with JSON files.
    /// </summary>
    public interface IEmbeddingProcessor
    {
        /// <summary>
        /// Asynchronously reads the content of a JSON file from the specified file path.
        /// </summary>
        /// <param name="jsonFilePath">The path to the JSON file that needs to be read.</param>
        /// <returns>A task representing the asynchronous operation. The result is the content of the JSON file as a string.</returns>
        Task<string> ReadJsonFileAsync(string jsonFilePath);

        /// <summary>
        /// Analyzes the content of the provided JSON string and processes it into a list of descriptions or data points.
        /// </summary>
        /// <param name="jsonContent">The JSON content to be analyzed.</param>
        /// <returns>A list of strings representing the analysis of the JSON content.</returns>
        List<string> AnalyzeJson(string jsonContent);  // Keep original method

        /// <summary>
        /// Processes the entire JSON content and transforms it into a more usable format, potentially for further processing or embedding generation.
        /// </summary>
        /// <param name="jsonContent">The JSON content to be processed.</param>
        /// <returns>A list of strings representing the processed JSON content.</returns>
        List<string> ProcessWholeJson(string jsonContent);  // Add new method

        /// <summary>
        /// Generates embeddings for a list of descriptions using an external API and saves the results to a CSV file at regular intervals.
        /// </summary>
        /// <param name="apiKey">The API key used to authenticate the embedding generation request.</param>
        /// <param name="descriptions">A list of descriptions for which embeddings need to be generated.</param>
        /// <param name="csvFilePath">The path to the CSV file where the generated embeddings should be saved.</param>
        /// <param name="saveInterval">The interval at which the data should be saved to the CSV file (e.g., after every N descriptions).</param>
        /// <returns>A task representing the asynchronous operation. This method does not return any value.</returns>
        Task GenerateAndSaveEmbeddingsAsync(string apiKey, List<string> descriptions, string csvFilePath, int saveInterval);

        /// <summary>
        /// Asynchronously processes a JSON file, analyzes its content, generates embeddings, and saves the embeddings to a CSV file at regular intervals.
        /// </summary>
        /// <param name="jsonFilePath">The path to the JSON file to be processed.</param>
        /// <param name="csvFilePath">The path to the CSV file where the embeddings will be saved.</param>
        /// <param name="apiKey">The API key used to authenticate the embedding generation request.</param>
        /// <param name="saveInterval">The interval at which the data should be saved to the CSV file (e.g., after every N descriptions).</param>
        /// <returns>A task representing the asynchronous operation. This method does not return any value.</returns>
        Task ProcessJsonFileAsync(string jsonFilePath, string csvFilePath, string apiKey, int saveInterval);
    }
}
