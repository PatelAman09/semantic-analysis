using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

class Embedding
{
    // Function to check if a JSON file exists and read its content
    static string ReadJsonFile(string jsonFilePath)
    {
        // Check if the file exists
        if (!File.Exists(jsonFilePath))
        {
            throw new FileNotFoundException($"The specified JSON file does not exist: {jsonFilePath}");
        }

        // Read and return the content of the JSON file
        return File.ReadAllText(jsonFilePath);
    }

    // Function to analyze JSON structure and extract data as a list of strings
    static List<string> AnalyzeJson(string jsonContent)
    {
        var parsedJson = JsonConvert.DeserializeObject(jsonContent);
        List<string> extractedData = new List<string>();

        if (parsedJson == null)
        {
            throw new Exception("The provided JSON content is empty or malformed.");
        }

        // Recursive function to traverse JSON structure
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

        // Start traversing the parsed JSON
        Traverse(parsedJson);
        return extractedData;
    }

    // Function to call OpenAI API and get embeddings for a single string of text
    static async Task<string> GetEmbeddingAsync(string apiKey, string text)
    {
        using (HttpClient client = new HttpClient())
        {
            // Set API key in the authorization header
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            // Prepare the request body
            var requestBody = new
            {
                model = "text-embedding-ada-002",
                input = text
            };

            // Convert the request body to JSON
            string jsonBody = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            // Send the POST request to OpenAI API
            HttpResponseMessage response = await client.PostAsync("https://api.openai.com/v1/embeddings", content);

            // Handle unsuccessful responses
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"OpenAI API request failed: {response.StatusCode} - {response.ReasonPhrase}");
            }

            // Parse the response safely
            string responseString = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(responseString);

            // Ensure the result and data are not null before accessing
            if (result?.data == null || result.data.Count == 0)
            {
                throw new Exception("No embedding data received from OpenAI API.");
            }

            // Use the null-forgiving operator to suppress warnings (because we know data exists)
            return result.data[0]?.embedding?.ToString() ?? string.Empty;
        }
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
            // Write the CSV header
            writer.WriteLine("Data,Embedding");

            // Write each data-embedding pair
            for (int i = 0; i < data.Count; i++)
            {
                writer.WriteLine($"\"{data[i]}\",\"{embeddings[i]}\"");
            }
        }
    }

    // Main function to orchestrate the workflow
    static async Task Main(string[] args)
    {
        try
        {
            // Prompt the user for input file paths and API key
            Console.WriteLine("Enter the path to your JSON file:");
            string jsonFilePath = Console.ReadLine() ?? throw new ArgumentNullException("jsonFilePath cannot be null");

            Console.WriteLine("Enter the path to save the output CSV file:");
            string csvFilePath = Console.ReadLine() ?? throw new ArgumentNullException("csvFilePath cannot be null");

            Console.WriteLine("Enter your OpenAI API key:");
            string apiKey = Console.ReadLine() ?? throw new ArgumentNullException("apiKey cannot be null");

            // Step 1: Read and analyze the JSON file
            Console.WriteLine("Reading and analyzing JSON file...");
            string jsonContent = ReadJsonFile(jsonFilePath);
            List<string> analyzedData = AnalyzeJson(jsonContent);

            // Step 2: Generate embeddings for each extracted string
            Console.WriteLine("Generating embeddings...");
            List<string> embeddings = new List<string>();
            foreach (var data in analyzedData)
            {
                Console.WriteLine($"Processing: {data}");
                string embedding = await GetEmbeddingAsync(apiKey, data);
                embeddings.Add(embedding);
            }

            // Step 3: Save results to CSV
            Console.WriteLine("Saving results to CSV...");
            SaveToCsv(csvFilePath, analyzedData, embeddings);

            Console.WriteLine("Process completed successfully. Output saved to the CSV file.");
        }
        catch (Exception ex)
        {
            // Handle any errors during execution
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
//Continou