using System.Globalization;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI.Embeddings;
using Semantic_Analysis.Interfaces;

// Modified class to process JSON data with user input for processing method
public class EmbeddingProcessor : IEmbeddingProcessor
{
    // Reads JSON file content from the specified path
    public async Task<string> ReadJsonFileAsync(string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath))
            throw new FileNotFoundException($"The specified JSON file does not exist: {jsonFilePath}");

        return await File.ReadAllTextAsync(jsonFilePath);
    }

    // Processes JSON as a whole document
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

        // Return the entire document as a single string
        return new List<string> { jsonContent };
    }

    // Original method to break down JSON into elements
    public List<string> AnalyzeJson(string jsonContent)
    {
        var parsedJson = JsonConvert.DeserializeObject(jsonContent);
        var extractedData = new List<string>();

        if (parsedJson == null)
            throw new Exception("The provided JSON content is empty or malformed.");

        // Recursively traverses JSON structure to extract values with their paths
        void Traverse(object obj, string prefix = "")
        {
            if (obj is JObject jObject)
            {
                // Process each property in JSON objects
                foreach (var property in jObject.Properties())
                {
                    Traverse(property.Value, $"{prefix}{property.Name}: ");
                }
            }
            else if (obj is JArray jArray)
            {
                // Process each element in JSON arrays with index
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

    // Generates embedding with retry logic and exponential backoff
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

    // Processes text descriptions in batches and saves generated embeddings to CSV
    public async Task GenerateAndSaveEmbeddingsAsync(string apiKey, List<string> descriptions, string csvFilePath, int saveInterval)
    {
        Console.WriteLine($"Initializing OpenAI Embedding client... Output file: {csvFilePath}");

        var client = new EmbeddingClient("text-embedding-3-large", apiKey);

        // Open StreamWriter once (overwrites file). (CSV header remains untouched.)
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
                // Attempt batch generation of embeddings for the current batch.
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
                else
                {
                    // Fallback: If batch result count mismatches, process each description individually.
                    Console.WriteLine("Batch result count mismatch. Falling back to individual processing for this batch.");
                    foreach (var description in batch)
                    {
                        OpenAIEmbedding embedding = await GenerateEmbeddingWithRetryAsync(client, description);
                        float[] embeddingArray = embedding.ToFloats().ToArray();
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
            }
        }

        Console.WriteLine("All embeddings processed and saved.");
    }

    // Main processing method that handles the full workflow with user input for processing method
    public async Task ProcessJsonFileAsync(string jsonFilePath, string csvFilePath, string apiKey, int saveInterval)
    {
        try
        {
            // Ensure clean output file
            Console.WriteLine($"Ensuring {csvFilePath} is deleted before processing...");
            if (File.Exists(csvFilePath))
            {
                File.Delete(csvFilePath);
                Console.WriteLine($"Previous output file deleted: {csvFilePath}");
            }

            // Read JSON content
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

            // Generate and save embeddings
            Console.WriteLine("Starting embedding generation...");
            await GenerateAndSaveEmbeddingsAsync(apiKey, processedData, csvFilePath, saveInterval);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing JSON file '{jsonFilePath}': {ex.Message}");
        }
    }

    // Loads application configuration from appsettings.json
    public static IConfigurationRoot LoadConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }

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
}