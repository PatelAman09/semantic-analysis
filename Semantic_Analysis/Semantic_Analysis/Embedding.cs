using System;
using System.Collections.Generic;
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

    // Function to generate embedding using EmbeddingClient (adapted from the first code)
    static List<(string description, string embedding)> GetEmbeddings(string apiKey, List<string> descriptions)
    {
        Console.WriteLine("Generating embeddings...");
        EmbeddingClient client = new EmbeddingClient("text-embedding-3-small", apiKey);

        List<(string description, string embedding)> embeddings = new();

        foreach (string description in descriptions)
        {
            Console.WriteLine($"Processing: {description}");
            OpenAIEmbedding embedding = client.GenerateEmbedding(description);
            string embeddingString = string.Join(",", embedding.ToFloats().ToArray());
            embeddings.Add((description, embeddingString));
        }

        return embeddings;
    }

    // Function to save data and embeddings to a CSV file
    static void SaveToCsv(string csvFilePath, List<string> data, List<string> embeddings)
    {
        if (data.Count != embeddings.Count)
        {
            throw new Exception("Data and embeddings lists have different lengths.");
        }

        using (StreamWriter writer = new StreamWriter(csvFilePath))
        {
            writer.WriteLine("Data,Embedding");

            for (int i = 0; i < data.Count; i++)
            {
                writer.WriteLine($"\"{data[i]}\",\"{embeddings[i]}\"");
            }
        }
    }

    // Orchestrator function to process the JSON file and save results
    static async Task ProcessJsonFile(string jsonFilePath, string csvFilePath, string apiKey)
    {
        Console.WriteLine("Reading and analyzing JSON file...");
        string jsonContent = ReadJsonFile(jsonFilePath);
        List<string> analyzedData = AnalyzeJson(jsonContent);

        Console.WriteLine("Generating embeddings...");
        List<(string description, string embedding)> embeddings = GetEmbeddings(apiKey, analyzedData);

        // Extract the embeddings from the tuple list for saving
        List<string> embeddingStrings = new List<string>();
        foreach (var embed in embeddings)
        {
            embeddingStrings.Add(embed.embedding);
        }

        Console.WriteLine("Saving results to CSV...");
        SaveToCsv(csvFilePath, analyzedData, embeddingStrings);

        Console.WriteLine("Process completed successfully. Output saved to the CSV file.");
    }

    // Main function to collect user input and call the orchestrator
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Enter the path to your JSON file:");
            string jsonFilePath = Console.ReadLine() ?? throw new ArgumentNullException("jsonFilePath cannot be null");

            Console.WriteLine("Enter the path to save the output CSV file:");
            string csvFilePath = Console.ReadLine() ?? throw new ArgumentNullException("csvFilePath cannot be null");

            string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                            ?? throw new Exception("Environment variable 'OPENAI_API_KEY' is not set.");

            await ProcessJsonFile(jsonFilePath, csvFilePath, apiKey);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
