using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

class Embedding
{
    // Step 1: Analyze the JSON structure and flatten it into key-value pairs or readable strings
    static List<string> AnalyzeJson(string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath))
        {
            throw new FileNotFoundException($"The specified JSON file does not exist: {jsonFilePath}");
        }

        string jsonContent = File.ReadAllText(jsonFilePath);
        var parsedJson = JsonConvert.DeserializeObject(jsonContent);

        List<string> extractedData = new List<string>();

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

    // Step 2: Call OpenAI API to generate embeddings
    static async Task<string> GetEmbeddingAsync(string apiKey, string text)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var requestBody = new
            {
                model = "text-embedding-ada-002",
                input = text
            };

            string jsonBody = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("https://api.openai.com/v1/embeddings", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"OpenAI API request failed: {response.StatusCode} - {response.ReasonPhrase}");
            }

            string responseString = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(responseString);
            return result.data[0].embedding.ToString();
        }
    }

    // Step 3: Save extracted data and embeddings to CSV.
    static void SaveToCsv(string csvFilePath, List<string> data, List<string> embeddings)
    {
        using (StreamWriter writer = new StreamWriter(csvFilePath))
        {
            writer.WriteLine("Data,Embedding");

            for (int i = 0; i < data.Count; i++)
            {
                writer.WriteLine($"\"{data[i]}\",\"{embeddings[i]}\"");
            }
        }
    }

    // Main Function
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Enter the path to your JSON file:");
            string jsonFilePath = Console.ReadLine();

            Console.WriteLine("Enter the path to save the output CSV file:");
            string csvFilePath = Console.ReadLine();

            Console.WriteLine("Enter your OpenAI API key:");
            string apiKey = Console.ReadLine();

            // Step 1: Analyze the JSON file
            Console.WriteLine("Analyzing JSON file...");
            List<string> analyzedData = AnalyzeJson(jsonFilePath);

            // Step 2: Generate embeddings for each extracted piece of data.
            Console.WriteLine("Generating embeddings...");
            List<string> embeddings = new List<string>();
            foreach (var data in analyzedData)
            {
                Console.WriteLine($"Processing: {data}");
                string embedding = await GetEmbeddingAsync(apiKey, data);
                embeddings.Add(embedding);
            }

            // Step 3: Save results to a CSV file.
            Console.WriteLine("Saving results to CSV...");
            SaveToCsv(csvFilePath, analyzedData, embeddings);

            Console.WriteLine("Process completed successfully. Output saved to the CSV file.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}