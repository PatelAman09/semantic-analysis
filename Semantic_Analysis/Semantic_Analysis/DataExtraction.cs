using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;  // For reading appsettings.json
using Newtonsoft.Json;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Xml.Linq;

namespace Semantic_Analysis
{
    public class DataExtraction : IDataExtraction
    {
        public static void Main(string[] args)
        {
            // Load settings from appsettings.json
            var configuration = LoadConfiguration();

            // Get folder paths from configuration
            string dataPreprocessingPath = configuration["FilePaths:DataPreprocessing"];
            string preprocessedDataPath = configuration["FilePaths:PreprocessedData"];
            string extractedDataFileName = configuration["FilePaths:ExtractedDataFileName"];

            // Resolve paths
            string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            string absoluteDataPreprocessingPath = Path.Combine(projectRoot, dataPreprocessingPath);
            string absolutePreprocessedDataPath = Path.Combine(projectRoot, preprocessedDataPath);

            // Ensure ExtractedData folder exists
            if (!Directory.Exists(absolutePreprocessedDataPath))
            {
                Directory.CreateDirectory(absolutePreprocessedDataPath);
            }

            // Find the first file in the Preprocessing folder (this can be refined to be more specific if needed)
            string inputFilePath = GetFirstFileInDirectory(absoluteDataPreprocessingPath);

            if (string.IsNullOrEmpty(inputFilePath))
            {
                Console.WriteLine("No preprocessing files found in the folder.");
                return;
            }

            // Define output file path
            string outputFilePath = Path.Combine(absolutePreprocessedDataPath, extractedDataFileName);

            // Create an instance of DataExtraction
            IDataExtraction processor = new DataExtraction();
            List<string> extractedData = processor.ExtractDataFromFile(inputFilePath);
            extractedData = processor.CleanData(extractedData);

            // Save the cleaned data to a text file
            processor.SaveDataToText(outputFilePath, extractedData);

            Console.WriteLine($"Data extracted and saved to: {outputFilePath}");
        }

        // Load configuration from appsettings.json
        private static IConfiguration LoadConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
            configurationBuilder.AddJsonFile("appsettings.json");

            return configurationBuilder.Build();
        }

        // Implement ExtractDataFromFile method
        public List<string> ExtractDataFromFile(string filePath)
        {
            var fileContent = new List<string>();
            try
            {
                string fileExtension = Path.GetExtension(filePath).ToLower();

                // Extract data based on the file type
                switch (fileExtension)
                {
                    case ".txt":
                        fileContent = ExtractDataFromText(filePath);
                        break;
                    case ".csv":
                        fileContent = ExtractDataFromCsv(filePath);
                        break;
                    case ".json":
                        fileContent = ExtractDataFromJson(filePath);
                        break;
                    case ".xml":
                        fileContent = ExtractDataFromXml(filePath);
                        break;
                    case ".html":
                    case ".htm":
                        fileContent = ExtractDataFromHtml(filePath);
                        break;
                    case ".md":
                        fileContent = ExtractDataFromMarkdown(filePath);
                        break;
                    case ".pdf":
                        fileContent = ExtractDataFromPdf(filePath);
                        break;
                    default:
                        fileContent = ExtractRawData(filePath);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file {filePath}: {ex.Message}");
            }
            return fileContent;
        }

        // Extracts text from a plain text file
        public List<string> ExtractDataFromText(string filePath)
        {
            var data = new List<string>();
            try
            {
                var lines = File.ReadAllLines(filePath);
                data.AddRange(lines);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading Text file: {ex.Message}");
            }
            return data;
        }

        // Extracts data from a CSV file
        public List<string> ExtractDataFromCsv(string filePath)
        {
            var data = new List<string>();
            try
            {
                var lines = File.ReadAllLines(filePath);
                data.AddRange(lines);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading CSV file: {ex.Message}");
            }
            return data;
        }

        // Extracts data from JSON file
        public List<string> ExtractDataFromJson(string filePath)
        {
            var data = new List<string>();
            try
            {
                var json = File.ReadAllText(filePath);

                // Deserialize the JSON data as a list of strings
                var jsonArray = JsonConvert.DeserializeObject<List<string>>(json);

                if (jsonArray != null)
                {
                    data.AddRange(jsonArray);
                }
                else
                {
                    data.Add("Error: JSON content is null or could not be parsed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading JSON file: {ex.Message}");
            }
            return data;
        }

        // Extracts data from XML file
        public List<string> ExtractDataFromXml(string filePath)
        {
            var data = new List<string>();
            try
            {
                var xml = XDocument.Load(filePath);
                foreach (var element in xml.Descendants())
                {
                    data.Add($"{element.Name}: {element.Value}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading XML file: {ex.Message}");
            }
            return data;
        }

        // Extracts text data from a PDF file
        public List<string> ExtractDataFromPdf(string filePath)
        {
            var data = new List<string>();
            try
            {
                using (var reader = new PdfReader(filePath))
                using (var pdfDoc = new PdfDocument(reader))
                {
                    for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                    {
                        var page = pdfDoc.GetPage(i);
                        var strategy = new SimpleTextExtractionStrategy();
                        var text = PdfTextExtractor.GetTextFromPage(page, strategy);
                        data.Add(text);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading PDF file: {ex.Message}");
                data.Add($"Error: {ex.Message}");
            }
            return data;
        }

        // Extracts raw byte data from a file
        public List<string> ExtractRawData(string filePath)
        {
            var data = new List<string>();
            try
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                string rawContent = BitConverter.ToString(bytes.Take(100).ToArray());
                data.Add($"Raw Content (first 100 bytes): {rawContent}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading raw file: {ex.Message}");
                data.Add($"Error: {ex.Message}");
            }
            return data;
        }

        // Extracts text data from a Markdown file
        public List<string> ExtractDataFromMarkdown(string filePath)
        {
            var data = new List<string>();
            try
            {
                var markdownContent = File.ReadAllText(filePath);
                var textOnly = Regex.Replace(markdownContent, @"[#\*\-]\s?", " ").Replace("\n", " ").Trim();
                data.Add(textOnly);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading Markdown file: {ex.Message}");
            }
            return data;
        }

        // Extracts text data from an HTML file by removing HTML tags
        public List<string> ExtractDataFromHtml(string filePath)
        {
            var data = new List<string>();
            try
            {
                var htmlContent = File.ReadAllText(filePath);
                var textOnly = Regex.Replace(htmlContent, @"<[^>]+?>", " ").Replace("\n", " ").Trim();
                data.Add(textOnly);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading HTML file: {ex.Message}");
            }
            return data;
        }

        // Cleans extracted data by trimming whitespace, removing special characters, and converting to lowercase
        public List<string> CleanData(List<string> data)
        {
            var cleanedData = new List<string>();

            foreach (var line in data)
            {
                var cleanedLine = line.Trim();  // Trim whitespaces
                cleanedLine = Regex.Replace(cleanedLine, @"[^A-Za-z0-9\s]", ""); // Remove special characters
                cleanedLine = cleanedLine.ToLower(); // Convert to lowercase
                if (!string.IsNullOrEmpty(cleanedLine)) // Remove empty lines
                {
                    cleanedData.Add(cleanedLine);
                }
            }

            return cleanedData;
        }

        // Saves data to a text file
        public void SaveDataToText(string outputFilePath, List<string> data)
        {
            try
            {
                File.WriteAllLines(outputFilePath, data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data to text file: {ex.Message}");
            }
        }

        // Helper method to find the first file in a directory
        private static string GetFirstFileInDirectory(string directoryPath)
        {
            try
            {
                var files = Directory.GetFiles(directoryPath);
                return files.FirstOrDefault(); // Returns the first file, or null if no files are found
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing the directory: {ex.Message}");
                return null;
            }
        }
    }
}
