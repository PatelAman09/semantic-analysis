using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI.Embeddings;
using Semantic_Analysis.Interfaces;

namespace Semantic_Analysis
{
    public class EmbeddingProcessor : IEmbeddingProcessor
    {
        private const string InputFolder = "C:\\Users\\ADMIN\\Desktop\\semester 1\\Software Eng\\Final Project\\Ahsan\\semantic-analysis\\Semantic_Analysis\\Semantic_Analysis\\ExtractedData\\";
        private const string OutputFolder = "C:\\Users\\ADMIN\\Desktop\\semester 1\\Software Eng\\Final Project\\Ahsan\\semantic-analysis\\Semantic_Analysis\\Semantic_Analysis\\Embeddingoutput\\";
        private const string OriginalFile = "original.json";
        private const string ReferenceFile = "reference.json";
        private const string OutputOriginalCsv = "embedding_original.csv";
        private const string OutputReferenceCsv = "embedding_reference.csv";

        public async Task<string> ReadJsonFileAsync(string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath))
                throw new FileNotFoundException($"File not found: {jsonFilePath}");

            return await File.ReadAllTextAsync(jsonFilePath);
        }

        public List<string> AnalyzeJson(string jsonContent)
        {
            var parsedJson = JsonConvert.DeserializeObject(jsonContent);
            var extractedData = new List<string>();

            void Traverse(object obj, string prefix = "")
            {
                if (obj is JObject jObject)
                {
                    foreach (var property in jObject.Properties())
                        Traverse(property.Value, $"{prefix}{property.Name}: ");
                }
                else if (obj is JArray jArray)
                {
                    for (int i = 0; i < jArray.Count; i++)
                        Traverse(jArray[i], $"{prefix}[{i}]: ");
                }
                else if (obj is JValue jValue)
                {
                    extractedData.Add($"{prefix}{jValue.Value}");
                }
            }

            Traverse(parsedJson);
            return extractedData;
        }

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
                    Console.WriteLine($"Attempt {attempt + 1} failed: {ex.Message}. Retrying...");
                    await Task.Delay(1000);
                    attempt++;
                }
            }
            throw new Exception("Failed to generate embedding after multiple attempts.");
        }

        public async Task GenerateAndSaveEmbeddingsAsync(string apiKey, List<string> descriptions, string csvFilePath, int saveInterval)
        {
            var client = new EmbeddingClient("text-embedding-3-small", apiKey);

            using StreamWriter writer = new StreamWriter(csvFilePath, append: false, encoding: Encoding.UTF8);
            await writer.WriteLineAsync("Description,Embedding");

            int count = 0;
            foreach (var description in descriptions)
            {
                try
                {
                    OpenAIEmbedding embedding = await GenerateEmbeddingWithRetryAsync(client, description);
                    float[] embeddingArray = embedding.ToFloats().ToArray();
                    string embeddingString = string.Join(",", embeddingArray.Select(e => e.ToString(CultureInfo.InvariantCulture)));
                    await writer.WriteLineAsync($"\"{description}\",\"{embeddingString}\"");

                    count++;
                    if (count % saveInterval == 0)
                    {
                        await writer.FlushAsync(); // Save progress at intervals
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing: {ex.Message}");
                }
            }
            Console.WriteLine($"Embeddings saved to: {csvFilePath}");
        }

        public async Task ProcessJsonFileAsync(string jsonFile, string outputCsv, string apiKey, int saveInterval)
        {
            try
            {
                string jsonFilePath = Path.Combine(InputFolder, jsonFile);
                string outputCsvPath = Path.Combine(OutputFolder, outputCsv);

                Console.WriteLine($"Processing {jsonFilePath}...");
                string jsonContent = await ReadJsonFileAsync(jsonFilePath);
                List<string> analyzedData = AnalyzeJson(jsonContent);

                await GenerateAndSaveEmbeddingsAsync(apiKey, analyzedData, outputCsvPath, saveInterval);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file {jsonFile}: {ex.Message}");
                throw;
            }
        }

        public static async Task Main()
        {
            while (true)
            {
                try
                {
                    string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new Exception("API key not found.");
                    EmbeddingProcessor processor = new EmbeddingProcessor();

                    // Ensure output directory exists
                    if (!Directory.Exists(OutputFolder))
                        Directory.CreateDirectory(OutputFolder);

                    // Clean overwrite: Delete old output files before processing
                    File.Delete(Path.Combine(OutputFolder, OutputOriginalCsv));
                    File.Delete(Path.Combine(OutputFolder, OutputReferenceCsv));

                    // Process both files
                    int saveInterval = 10; // Save after every 10 descriptions
                    await processor.ProcessJsonFileAsync(OriginalFile, OutputOriginalCsv, apiKey, saveInterval);
                    await processor.ProcessJsonFileAsync(ReferenceFile, OutputReferenceCsv, apiKey, saveInterval);

                    Console.WriteLine("Processing completed successfully.");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    Console.Write("Do you want to retry the full process? (y/n): ");
                    string response = Console.ReadLine()?.Trim().ToLower();
                    if (response != "y")
                    {
                        Console.WriteLine("Exiting program.");
                        break;
                    }
                }
            }
        }
    }
}
