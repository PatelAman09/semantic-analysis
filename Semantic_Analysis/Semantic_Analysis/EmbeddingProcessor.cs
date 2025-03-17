using System.Globalization;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI.Embeddings;
using Semantic_Analysis.Interfaces;

// Class for processing JSON data and generating OpenAI embeddings
public class EmbeddingProcessor : IEmbeddingProcessor
{
    // Reads JSON file content from the specified path
    public async Task<string> ReadJsonFileAsync(string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath))
            throw new FileNotFoundException($"The specified JSON file does not exist: {jsonFilePath}");

        return await File.ReadAllTextAsync(jsonFilePath);
    }

    // Extracts data elements from JSON by traversing the structure recursively
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
                // Exponential backoff: 1s, 2s, 3s, etc.
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

    // Saves processed data to a CSV file
    public void SaveOutputToCsv(string outputFilePath, List<string> outputData)
    {
        try
        {
            Console.WriteLine($"Attempting to write {outputData.Count} lines to {outputFilePath}");
            File.WriteAllLines(outputFilePath, outputData);
            Console.WriteLine($"File successfully saved: {outputFilePath}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error saving output file '{outputFilePath}': {ex.Message}");
        }
    }

    // Main processing method that handles the full workflow from JSON to embeddings
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

            // Extract data from JSON
            Console.WriteLine("Reading and analyzing JSON file...");
            string jsonContent = await ReadJsonFileAsync(jsonFilePath);
            List<string> analyzedData = AnalyzeJson(jsonContent);

            // Generate and save embeddings
            Console.WriteLine("Starting embedding generation...");
            await GenerateAndSaveEmbeddingsAsync(apiKey, analyzedData, csvFilePath, saveInterval);
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

}