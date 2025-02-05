using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI.Embeddings; //OpenAI NuGet package 

class EmbeddingProcessor
{
    static async Task<string> ReadJsonFileAsync(string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath))
        {
            throw new FileNotFoundException($"The specified JSON file does not exist: {jsonFilePath}");
        }

        return await File.ReadAllTextAsync(jsonFilePath);
    }

    static List<string> AnalyzeJson(string jsonContent)
    {
        var parsedJson = JsonConvert.DeserializeObject(jsonContent);
        var extractedData = new List<string>();

        if (parsedJson == null)
        {
            throw new Exception("The provided JSON content is empty or malformed.");
        }

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
                int index = 0;
                foreach (var item in jArray)
                {
                    Traverse(item, $"{prefix}[{index++}]: ");
                }
            }
            else if (obj is JValue jValue)
            {
                extractedData.Add($"{prefix}{jValue.Value}");
            }
        }

        Traverse(parsedJson);
        return extractedData;
    }

    static async Task<OpenAIEmbedding> GenerateEmbeddingWithRetryAsync(EmbeddingClient client, string text, int maxRetries = 3)
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
                Console.WriteLine($"Attempt {attempt + 1} failed: {ex.Message}. Retrying...");
                await Task.Delay(1000);
                attempt++;
            }
        }
        throw new Exception("Failed to generate embedding after multiple attempts.");
    }

    static async Task GenerateAndSaveEmbeddingsAsync(string apiKey, List<string> descriptions, string csvFilePath, int saveInterval)
    {
        Console.WriteLine("Initializing embedding generation...");
        var client = new EmbeddingClient("text-embedding-3-small", apiKey);
        bool fileExists = File.Exists(csvFilePath);

        using StreamWriter writer = new StreamWriter(csvFilePath, append: true, encoding: Encoding.UTF8);

        if (!fileExists)
        {
            await writer.WriteLineAsync("Description,Embedding");
        }

        for (int i = 0; i < descriptions.Count; i++)
        {
            try
            {
                string description = descriptions[i];
                Console.WriteLine($"Processing entry {i + 1}/{descriptions.Count}: {description}");

                OpenAIEmbedding embedding = await GenerateEmbeddingWithRetryAsync(client, description);

                // ReadOnlyMemory<float> to an array before async processing
                float[] embeddingArray = embedding.ToFloats().ToArray();
                string embeddingString = string.Join(",", embeddingArray.Select(e => e.ToString(CultureInfo.InvariantCulture)));

                await writer.WriteLineAsync($"\"{description}\",\"{embeddingString}\"");

                if ((i + 1) % saveInterval == 0)
                {
                    await writer.FlushAsync();
                    Console.WriteLine($"Checkpoint reached: {i + 1} embeddings saved.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing entry {i + 1}: {ex.Message}");
            }
        }

        Console.WriteLine("All embeddings processed and saved.");
    }

    static async Task ProcessJsonFileAsync(string jsonFilePath, string csvFilePath, string apiKey, int saveInterval)
    {
        try
        {
            Console.WriteLine("Reading and analyzing JSON file...");
            string jsonContent = await ReadJsonFileAsync(jsonFilePath);
            List<string> analyzedData = AnalyzeJson(jsonContent);

            Console.WriteLine("Starting embedding generation...");
            await GenerateAndSaveEmbeddingsAsync(apiKey, analyzedData, csvFilePath, saveInterval);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    //static async Task Main(string[] args)
    //{
    //    try
    //    {
    //        Console.WriteLine("Enter the path to your JSON file:");
    //        string jsonFilePath = Console.ReadLine() ?? throw new ArgumentNullException("JSON file path cannot be null.");

    //        Console.WriteLine("Enter the path to save the output CSV file:");
    //        string csvFilePath = Console.ReadLine() ?? throw new ArgumentNullException("CSV file path cannot be null.");

    //        Console.WriteLine("Enter the save interval (e.g., 10):");
    //        if (!int.TryParse(Console.ReadLine(), out int saveInterval) || saveInterval <= 0)
    //        {
    //            throw new ArgumentException("Save interval must be a positive integer.");
    //        }

    //        string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
    //                        ?? throw new Exception("Environment variable 'OPENAI_API_KEY' is not set.");

    //        await ProcessJsonFileAsync(jsonFilePath, csvFilePath, apiKey, saveInterval);
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"An error occurred: {ex.Message}");
    //    }
    //}
}
