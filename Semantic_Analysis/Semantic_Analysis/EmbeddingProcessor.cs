using System.Globalization;
using System.Text;
using System.Numerics;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI.Embeddings;
using Semantic_Analysis.Interfaces;

public class EmbeddingProcessor : IEmbeddingProcessor
{
    #region Constants
    private const int MaxTokens = 8000;
    private const int EmbeddingDimension = 3072;
    #endregion

    #region File Operations
    /// <summary>
    /// Reads JSON file content from the specified path.
    /// </summary>
    /// <param name="jsonFilePath">The path to the JSON file to read.</param>
    /// <returns>The content of the JSON file as a string.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
    public async Task<string> ReadJsonFileAsync(string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath))
            throw new FileNotFoundException($"The specified JSON file does not exist: {jsonFilePath}");

        return await File.ReadAllTextAsync(jsonFilePath);
    }

    public static IConfigurationRoot LoadConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }
    #endregion

    #region Text Processing
    /// <summary>
    /// Splits text into chunks that fit within token limits to handle large texts.
    /// </summary>
    /// <param name="text">The text to be split into chunks.</param>
    /// <returns>A list of string chunks that are within the token limit.</returns>
    private List<string> SplitIntoChunks(string text)
    {
        // Approximation: ~4 chars per token
        int charsPerToken = 4;
        int maxChars = MaxTokens * charsPerToken;

        List<string> chunks = new List<string>();

        for (int i = 0; i < text.Length; i += maxChars)
        {
            int length = Math.Min(maxChars, text.Length - i);
            chunks.Add(text.Substring(i, length));
        }

        return chunks;
    }
    #endregion

    #region JSON File Processing
    /// <summary>
    /// Processes JSON as a whole document for embedding generation.
    /// Validates JSON format and determines if chunking is needed based on size.
    /// </summary>
    /// <param name="jsonContent">The JSON content to process.</param>
    /// <returns>A list containing the JSON content, possibly as a single item.</returns>
    /// <exception cref="Exception">Thrown when the JSON content is empty or malformed.</exception>
    public List<string> ProcessWholeJson(string jsonContent)
    {
        try
        {
            JsonConvert.DeserializeObject(jsonContent);
        }
        catch (Exception ex)
        {
            throw new Exception($"The provided JSON content is empty or malformed: {ex.Message}");
        }

        int estimatedTokens = jsonContent.Length / 4; 

        if (estimatedTokens > MaxTokens)
        {
            Console.WriteLine($"Content is too large (est. {estimatedTokens} tokens). Will process in chunks and average embeddings.");
            return new List<string> { jsonContent }; // Return as one item, chunking will be handled during embedding
        }

        // If small enough, return as a single chunk
        return new List<string> { jsonContent };
    }

    /// <summary>
    /// Breaks down JSON into individual elements with their full paths.
    /// Recursively traverses the JSON structure to extract all values.
    /// </summary>
    /// <param name="jsonContent">The JSON content to analyze.</param>
    /// <returns>A list of strings representing individual JSON elements with their paths.</returns>
    /// <exception cref="Exception">Thrown when the JSON content is empty or malformed.</exception>
    public List<string> AnalyzeJson(string jsonContent)
    {
        var parsedJson = JsonConvert.DeserializeObject(jsonContent);
        var extractedData = new List<string>();

        if (parsedJson == null)
            throw new Exception("The provided JSON content is empty or malformed.");

        void Traverse(object obj, string prefix = "")
        {
            if (obj is JObject jObject)
            {
                foreach (var property in jObject.Properties())
                {
                    Traverse(property.Value, $"{prefix}{property.Name}: ");
                }
            }
            else if (obj is JArray jArray)
            {
                for (int i = 0; i < jArray.Count; i++)
                {
                    Traverse(jArray[i], $"{prefix}[{i}]: ");
                }
            }
            else if (obj is JValue jValue)
            {
                // Add leaf node values to the extracted data
                extractedData.Add($"{prefix.TrimEnd(' ')}{jValue.Value}");
            }
        }

        Traverse(parsedJson);
        return extractedData;
    }
    #endregion

    #region Embedding Generation
    /// <summary>
    /// Generates embedding with retry logic and exponential backoff for handling API errors.
    /// </summary>
    /// <param name="client">The OpenAI embedding client.</param>
    /// <param name="text">The text to generate embedding for.</param>
    /// <param name="maxRetries">Maximum number of retry attempts (default: 3).</param>
    /// <returns>The generated embedding object.</returns>
    /// <exception cref="Exception">Thrown when embedding generation fails after all retries.</exception>
    public async Task<OpenAIEmbedding> GenerateEmbeddingWithRetryAsync(EmbeddingClient client, string text, int maxRetries = 3)
    {
        int attempt = 0;
        while (attempt < maxRetries)
        {
            try
            {
                return await client.GenerateEmbeddingAsync(text);
            }
            catch (Exception ex) when (attempt < maxRetries - 1)
            {
                // Log a snippet of the text (max 50 characters) for context
                string snippet = text.Length > 50 ? text.Substring(0, 50) + "..." : text;
                Console.WriteLine($"Attempt {attempt + 1} failed for text: \"{snippet}\": {ex.Message}. Retrying...");
                attempt++;
                // Exponential backoff delay between retries
                await Task.Delay((attempt) * 1000);
            }
        }
        throw new Exception("Failed to generate embedding after multiple attempts.");
    }

    /// <summary>
    /// Calculates the average of multiple embeddings by summing and then dividing by count.
    /// </summary>
    /// <param name="embeddings">List of embedding vectors to average.</param>
    /// <returns>A new embedding vector that is the average of all input embeddings.</returns>
    /// <exception cref="ArgumentException">Thrown when the list of embeddings is empty.</exception>
    private float[] AverageEmbeddings(List<float[]> embeddings)
    {
        if (embeddings.Count == 0)
            throw new ArgumentException("Cannot average empty list of embeddings");

        float[] result = new float[EmbeddingDimension];

        // Sum all embeddings
        foreach (var embedding in embeddings)
        {
            for (int i = 0; i < EmbeddingDimension; i++)
            {
                result[i] += embedding[i];
            }
        }

        // Divide by count to get average
        for (int i = 0; i < EmbeddingDimension; i++)
        {
            result[i] /= embeddings.Count;
        }

        return result;
    }

    /// <summary>
    /// Generates embedding for potentially large text by splitting and averaging.
    /// If text exceeds token limit, it's split into chunks and the resulting embeddings are averaged.
    /// </summary>
    /// <param name="client">The OpenAI embedding client.</param>
    /// <param name="text">The text to generate embedding for.</param>
    /// <returns>An embedding vector for the input text.</returns>
    private async Task<float[]> GenerateChunkedEmbeddingAsync(EmbeddingClient client, string text)
    {
        int estimatedTokens = text.Length / 4;

        // If text is small enough, generate embedding directly
        if (estimatedTokens <= MaxTokens)
        {
            OpenAIEmbedding embedding = await GenerateEmbeddingWithRetryAsync(client, text);
            return embedding.ToFloats().ToArray();
        }

        // Split into chunks
        List<string> chunks = SplitIntoChunks(text);
        Console.WriteLine($"Processing document in {chunks.Count} chunks.");

        List<float[]> chunkEmbeddings = new List<float[]>();
        foreach (string chunk in chunks)
        {
            OpenAIEmbedding embedding = await GenerateEmbeddingWithRetryAsync(client, chunk);
            chunkEmbeddings.Add(embedding.ToFloats().ToArray());
        }

        return AverageEmbeddings(chunkEmbeddings);
    }
    #endregion

    #region Embedding Generation with Batch Processing
    /// <summary>
    /// Processes text descriptions in batches and saves generated embeddings to CSV.
    /// Handles large inputs by chunking and includes progress tracking with checkpoints.
    /// </summary>
    /// <param name="apiKey">The OpenAI API key.</param>
    /// <param name="descriptions">List of text descriptions to generate embeddings for.</param>
    /// <param name="csvFilePath">Path to the output CSV file.</param>
    /// <param name="saveInterval">Interval at which to save progress and report status.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task GenerateAndSaveEmbeddingsAsync(string apiKey, List<string> descriptions, string csvFilePath, int saveInterval)
    {
        Console.WriteLine($"Initializing OpenAI Embedding client... Output file: {csvFilePath}");

        var client = new EmbeddingClient("text-embedding-3-large", apiKey);

        using StreamWriter writer = new StreamWriter(csvFilePath, append: false, encoding: Encoding.UTF8);
        Console.WriteLine($"Overwriting CSV file: {csvFilePath}");

        // Define a batch size for processing. Adjust this value based on your workload and API limits.
        int batchSize = 10;
        int processedCount = 0;

        for (int i = 0; i < descriptions.Count; i += batchSize)
        {
            List<string> batch = descriptions.Skip(i).Take(batchSize).ToList();
            try
            {
                bool batchHasLargeItems = batch.Any(text => text.Length / 4 > MaxTokens);

                if (!batchHasLargeItems)
                {
                    OpenAIEmbeddingCollection embeddingResults = await client.GenerateEmbeddingsAsync(batch.ToArray());

                    if (embeddingResults != null && embeddingResults.Count == batch.Count)
                    {
                        for (int j = 0; j < batch.Count; j++)
                        {
                            string description = batch[j];
                            float[] embeddingArray = embeddingResults[j].ToFloats().ToArray();
                            string embeddingString = string.Join(",", embeddingArray.Select(e => e.ToString(CultureInfo.InvariantCulture)));
                            await writer.WriteLineAsync($"\"{description}\",\"{embeddingString}\"");
                            processedCount++;

                            if (processedCount % saveInterval == 0)
                            {
                                await writer.FlushAsync();
                                Console.WriteLine($"Checkpoint reached: {processedCount} embeddings saved.");
                            }
                        }
                    }
                }
                else
                {
                    foreach (var description in batch)
                    {
                        float[] embeddingArray;

                        if (description.Length / 4 > MaxTokens)
                        {
                            embeddingArray = await GenerateChunkedEmbeddingAsync(client, description);
                        }
                        else
                        {
                            OpenAIEmbedding embedding = await GenerateEmbeddingWithRetryAsync(client, description);
                            embeddingArray = embedding.ToFloats().ToArray();
                        }

                        string embeddingString = string.Join(",", embeddingArray.Select(e => e.ToString(CultureInfo.InvariantCulture)));
                        await writer.WriteLineAsync($"\"{description}\",\"{embeddingString}\"");
                        processedCount++;

                        // Periodically save progress
                        if (processedCount % saveInterval == 0)
                        {
                            await writer.FlushAsync();
                            Console.WriteLine($"Checkpoint reached: {processedCount} embeddings saved.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.UtcNow}] Error processing batch starting at entry {i + 1}: {ex.Message}");

                // Fallback: Process each item individually if batch processing fails
                foreach (var description in batch)
                {
                    try
                    {
                        float[] embeddingArray;

                        if (description.Length / 4 > MaxTokens)
                        {
                            embeddingArray = await GenerateChunkedEmbeddingAsync(client, description);
                        }
                        else
                        {
                            OpenAIEmbedding embedding = await GenerateEmbeddingWithRetryAsync(client, description);
                            embeddingArray = embedding.ToFloats().ToArray();
                        }

                        string embeddingString = string.Join(",", embeddingArray.Select(e => e.ToString(CultureInfo.InvariantCulture)));
                        await writer.WriteLineAsync($"\"{description}\",\"{embeddingString}\"");
                        processedCount++;

                        if (processedCount % saveInterval == 0)
                        {
                            await writer.FlushAsync();
                            Console.WriteLine($"Checkpoint reached: {processedCount} embeddings saved.");
                        }
                    }
                    catch (Exception itemEx)
                    {
                        Console.WriteLine($"[{DateTime.UtcNow}] Error processing individual item: {itemEx.Message}");
                    }
                }
            }
        }

        Console.WriteLine("All embeddings processed and saved.");
    }
    #endregion

    #region DataEmbeddingPipeline
    /// <summary>
    /// Main processing method that handles the full workflow with user input for processing method.
    /// Manages the entire process from JSON file reading to generating and saving embeddings.
    /// </summary>
    /// <param name="jsonFilePath">Path to the input JSON file.</param>
    /// <param name="csvFilePath">Path to the output CSV file for embeddings.</param>
    /// <param name="apiKey">The OpenAI API key.</param>
    /// <param name="saveInterval">Interval at which to save progress and report status.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ProcessJsonFileAsync(string jsonFilePath, string csvFilePath, string apiKey, int saveInterval)
    {
        try
        {
            Console.WriteLine($"Ensuring {csvFilePath} is deleted before processing...");
            if (File.Exists(csvFilePath))
            {
                File.Delete(csvFilePath);
                Console.WriteLine($"Previous output file deleted: {csvFilePath}");
            }

            Console.WriteLine($"Reading JSON file: {jsonFilePath}");
            string jsonContent = await ReadJsonFileAsync(jsonFilePath);

            // Ask user for processing preference for this file
            Console.WriteLine($"\nHow would you like to process {Path.GetFileName(jsonFilePath)}?");
            Console.WriteLine("1. Break down into individual elements (words/phrases)");
            Console.WriteLine("2. Process as a whole document (single embedding)");
            Console.Write("Enter your choice (1 or 2): ");
            string choice = Console.ReadLine();

            List<string> processedData;
            if (choice == "2")
            {
                Console.WriteLine("Processing as a whole document...");
                processedData = ProcessWholeJson(jsonContent);
            }
            else
            {
                Console.WriteLine("Breaking down into individual elements...");
                processedData = AnalyzeJson(jsonContent);
            }

            Console.WriteLine($"Extracted {processedData.Count} items for processing.");

            Console.WriteLine("Starting embedding generation...");
            await GenerateAndSaveEmbeddingsAsync(apiKey, processedData, csvFilePath, saveInterval);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing JSON file '{jsonFilePath}': {ex.Message}");
        }
    }
    #endregion

    #region Main Method (Commented Out)
    //static async Task Main(string[] args)
    //{
    //    try
    //    {
    //        IConfigurationRoot config = LoadConfiguration();
    //        string inputFolder = config["FilePaths:ExtractedData"];
    //        string outputFolder = config["FilePaths:InputFolder"];

    //        string rootDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
    //        string outputFile1 = Path.Combine(rootDirectory, outputFolder, config["FilePaths:InputFileName1"]);
    //        string outputFile2 = Path.Combine(rootDirectory, outputFolder, config["FilePaths:InputFileName2"]);

    //        string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
    //            ?? throw new Exception("Environment variable 'OPENAI_API_KEY' is not set.");

    //        if (!Directory.Exists(inputFolder))
    //            throw new DirectoryNotFoundException($"Input folder not found: {inputFolder}");
    //        if (!Directory.Exists(outputFolder))
    //        {
    //            Console.WriteLine($"Output folder not found. Creating: {outputFolder}");
    //            Directory.CreateDirectory(outputFolder);
    //        }

    //        // Debug output: show working directory and absolute file paths.
    //        Console.WriteLine($"Current Working Directory: {Directory.GetCurrentDirectory()}");
    //        Console.WriteLine($"Output File 1: {Path.GetFullPath(outputFile1)}");
    //        Console.WriteLine($"Output File 2: {Path.GetFullPath(outputFile2)}");

    //        Console.WriteLine("Cleaning up previous output files...");
    //        if (File.Exists(outputFile1)) File.Delete(outputFile1);
    //        if (File.Exists(outputFile2)) File.Delete(outputFile2);
    //        Console.WriteLine("Old output files deleted.");

    //        var jsonFiles = Directory.GetFiles(inputFolder, "*.json");
    //        if (jsonFiles.Length < 2)
    //            throw new Exception("Expected at least two JSON files in the input folder.");

    //        Console.WriteLine($"Found JSON files: {string.Join(", ", jsonFiles.Select(Path.GetFileName))}");

    //        // Create processor
    //        IEmbeddingProcessor processor = new EmbeddingProcessor();

    //        // Process files sequentially to allow user input for each file
    //        Console.WriteLine($"\nProcessing first file: {jsonFiles[0]}");
    //        await processor.ProcessJsonFileAsync(jsonFiles[0], outputFile1, apiKey, 10);

    //        Console.WriteLine($"\nProcessing second file: {jsonFiles[1]}");
    //        await processor.ProcessJsonFileAsync(jsonFiles[1], outputFile2, apiKey, 10);

    //        Console.WriteLine("Embedding processing completed successfully.");
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"Error: {ex.Message}");
    //    }
    //}
    #endregion
}