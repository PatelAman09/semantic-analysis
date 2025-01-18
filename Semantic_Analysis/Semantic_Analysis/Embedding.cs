using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI.Embeddings; // NuGet package for embeddings

class Embedding
{
    // Function to read JSON content from a file
    static string ReadJsonFile(string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath))
        {
            throw new FileNotFoundException($"The specified JSON file does not exist: {jsonFilePath}");
        }

        return File.ReadAllText(jsonFilePath);
    }

    // Function to analyze JSON content and extract data
    static List<string> AnalyzeJson(string jsonContent)
    {
        var parsedJson = JsonConvert.DeserializeObject(jsonContent);
        var extractedData = new List<string>();

        if (parsedJson == null)
        {
            throw new Exception("The provided JSON content is empty or malformed.");
        }

        void Traverse(object? obj, string prefix = "")
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

    // Function to generate embeddings using EmbeddingClient
    static async Task GenerateAndSaveEmbeddingsAsync(string apiKey, List<string> descriptions, string csvFilePath, int saveInterval)
    {
        Console.WriteLine("Generating embeddings...");
        EmbeddingClient client = new EmbeddingClient("text-embedding-3-small", apiKey);

        // Prepare CSV file
        using StreamWriter writer = new StreamWriter(csvFilePath, append: true);
        if (new FileInfo(csvFilePath).Length == 0)
        {
            writer.WriteLine("Description,Embedding"); // Write header only if the file is empty
        }

        for (int i = 0; i < descriptions.Count; i++)
        {
            try
            {
                Console.WriteLine($"Processing: {descriptions[i]}");
                OpenAIEmbedding embedding = client.GenerateEmbedding(descriptions[i]);

                // Convert ReadOnlyMemory<float> to array and create a string with periods (.) separating the vector components
                var embeddingArray = embedding.ToFloats().ToArray();
                var embeddingString = string.Join(",", embeddingArray.Select(e => e.ToString(CultureInfo.InvariantCulture)));  // Use InvariantCulture to force period as decimal separator

                // Save the embedding immediately
                writer.WriteLine($"\"{descriptions[i]}\",\"{embeddingString}\"");

                // Save progress after the specified interval
                if ((i + 1) % saveInterval == 0)
                {
                    writer.Flush(); // Ensure data is written to the file
                    Console.WriteLine($"Checkpoint: Saved {i + 1} embeddings.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing description at index {i}: {ex.Message}");
            }
        }

        Console.WriteLine("All embeddings processed and saved.");
    }

    // Orchestrator function to process the JSON file and save results
    static async Task ProcessJsonFileAsync(string jsonFilePath, string csvFilePath, string apiKey, int saveInterval)
    {
        Console.WriteLine("Reading and analyzing JSON file...");
        string jsonContent = ReadJsonFile(jsonFilePath);
        List<string> analyzedData = AnalyzeJson(jsonContent);

        Console.WriteLine("Starting embedding generation...");
        await GenerateAndSaveEmbeddingsAsync(apiKey, analyzedData, csvFilePath, saveInterval);
    }

    // Main function to collect user input and call the orchestrator
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Enter the path to your JSON file:");
            string jsonFilePath = Console.ReadLine() ?? throw new ArgumentNullException("JSON file path cannot be null.");

            Console.WriteLine("Enter the path to save the output CSV file:");
            string csvFilePath = Console.ReadLine() ?? throw new ArgumentNullException("CSV file path cannot be null.");

            Console.WriteLine("Enter the save interval (e.g., 10):");
            if (!int.TryParse(Console.ReadLine(), out int saveInterval) || saveInterval <= 0)
            {
                throw new ArgumentException("Save interval must be a positive integer.");
            }

            string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                            ?? throw new Exception("Environment variable 'OPENAI_API_KEY' is not set.");

            await ProcessJsonFileAsync(jsonFilePath, csvFilePath, apiKey, saveInterval);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
