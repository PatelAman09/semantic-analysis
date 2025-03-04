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
            // Load configuration settings from appsettings.json
            var configuration = LoadConfiguration();

            // Retrieve folder paths and file names from configuration
            string dataPreprocessingPath = configuration["FilePaths:DataPreprocessing"];
            string preprocessedDataPath = configuration["FilePaths:PreprocessedData"];
            string referenceDataPath = configuration["FilePaths:ReferenceData"];
            var extractedDataFileNames = configuration.GetSection("FilePaths:ExtractedDataFileNames").GetChildren().Select(x => x.Value).ToList();
            var referenceDocumentFileNames = configuration.GetSection("FilePaths:ReferenceDocumentFileNames").GetChildren().Select(x => x.Value).ToList();

            // Resolve the absolute paths for the directories
            string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            string absoluteDataPreprocessingPath = Path.Combine(projectRoot, dataPreprocessingPath);
            string absolutePreprocessedDataPath = Path.Combine(projectRoot, preprocessedDataPath);
            string absoluteReferenceDataPath = Path.Combine(projectRoot, referenceDataPath);

            // Ensure the necessary directories exist
            EnsureDirectoryExists(absolutePreprocessedDataPath);
            EnsureDirectoryExists(absoluteReferenceDataPath);

            // Locate the required extracted and reference data files
            string extractedDataFilePath = FindFileInDirectory(absoluteDataPreprocessingPath, extractedDataFileNames);
            string referenceDocumentFilePath = FindFileInDirectory(absoluteDataPreprocessingPath, referenceDocumentFileNames);

            // Validate file existence and print appropriate messages if not found
            if (string.IsNullOrEmpty(extractedDataFilePath))
            {
                Console.WriteLine("No extracted data file found.");
                return;
            }

            if (string.IsNullOrEmpty(referenceDocumentFilePath))
            {
                Console.WriteLine("No reference document file found.");
                return;
            }

            // Define the output file paths for the processed data
            string outputExtractedDataFilePath = Path.Combine(absolutePreprocessedDataPath, $"{Path.GetFileNameWithoutExtension(extractedDataFilePath)}.json");
            string outputReferenceDocumentFilePath = Path.Combine(absoluteReferenceDataPath, $"{Path.GetFileNameWithoutExtension(referenceDocumentFilePath)}.json");

            // Create an instance of DataExtraction to process the files
            IDataExtraction processor = new DataExtraction();

            // Process the extracted data file
            List<string> extractedData = processor.ExtractDataFromFile(extractedDataFilePath);
            extractedData = processor.CleanData(extractedData);
            processor.SaveDataToJson(outputExtractedDataFilePath, extractedData, "extracted");

            // Process the reference document file
            List<string> referenceData = processor.ExtractDataFromFile(referenceDocumentFilePath);
            referenceData = processor.CleanData(referenceData);
            processor.SaveDataToJson(outputReferenceDocumentFilePath, referenceData, "reference");


            // Output the result of the data extraction
            Console.WriteLine($"Data extracted and saved to: {outputExtractedDataFilePath}");
            Console.WriteLine($"Reference document data extracted and saved to: {outputReferenceDocumentFilePath}");
        }

        private static IConfiguration LoadConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
            configurationBuilder.AddJsonFile("appsettings.json");

            return configurationBuilder.Build();
        }

        private static void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        private static string FindFileInDirectory(string directoryPath, List<string> fileNames)
        {
            foreach (var fileName in fileNames)
            {
                var files = Directory.GetFiles(directoryPath, fileName);
                if (files.Any())
                {
                    return files.First(); // Return the first matching file
                }
            }

            return null; // Return null if no matching file is found
        }

        public List<string> ExtractDataFromFile(string filePath)
        {
            var fileContent = new List<string>();
            try
            {
                string fileExtension = Path.GetExtension(filePath).ToLower();

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

        // --- Extraction Methods ---
        public List<string> ExtractDataFromText(string filePath) => File.ReadAllLines(filePath).ToList();
        public List<string> ExtractDataFromCsv(string filePath) => File.ReadAllLines(filePath).ToList();
        public List<string> ExtractDataFromJson(string filePath)
        {
            try
            {
                var json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string> { "Error: JSON content is null or could not be parsed." };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading JSON file: {ex.Message}");
                return new List<string> { $"Error: {ex.Message}" };
            }
        }
        public List<string> ExtractDataFromXml(string filePath)
        {
            try
            {
                var xml = XDocument.Load(filePath);
                return xml.Descendants().Select(element => $"{element.Name}: {element.Value}").ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading XML file: {ex.Message}");
                return new List<string> { $"Error: {ex.Message}" };
            }
        }
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
        public List<string> ExtractRawData(string filePath)
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                string rawContent = BitConverter.ToString(bytes.Take(100).ToArray());
                return new List<string> { $"Raw Content (first 100 bytes): {rawContent}" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading raw file: {ex.Message}");
                return new List<string> { $"Error: {ex.Message}" };
            }
        }
        public List<string> ExtractDataFromMarkdown(string filePath)
        {
            try
            {
                var markdownContent = File.ReadAllText(filePath);
                var textOnly = Regex.Replace(markdownContent, @"[#\*\-]\s?", " ").Replace("\n", " ").Trim();
                return new List<string> { textOnly };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading Markdown file: {ex.Message}");
                return new List<string> { $"Error: {ex.Message}" };
            }
        }
        public List<string> ExtractDataFromHtml(string filePath)
        {
            try
            {
                var htmlContent = File.ReadAllText(filePath);
                var textOnly = Regex.Replace(htmlContent, @"<[^>]+?>", " ").Replace("\n", " ").Trim();
                return new List<string> { textOnly };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading HTML file: {ex.Message}");
                return new List<string> { $"Error: {ex.Message}" };
            }
        }

        // --- Data Cleaning Methods ---
        public List<string> CleanData(List<string> data)
        {
            var cleanedData = new List<string>();

            foreach (var line in data)
            {
                // Split into sentences by punctuation marks (., !, ?)
                var sentences = Regex.Split(line, @"(?<=[.!?])\s+");

                foreach (var sentence in sentences)
                {
                    var cleanedSentence = sentence
                        .Trim()  // Remove leading/trailing spaces
                        .ToLower() // Convert to lowercase
                        .Replace(",", "") // Remove commas
                        .Replace(".", "") // Remove periods
                        .Replace("!", "") // Remove exclamations
                        .Replace("?", ""); // Remove question marks

                    if (!string.IsNullOrEmpty(cleanedSentence))
                    {
                        cleanedData.Add(cleanedSentence); // Add each cleaned sentence to list
                    }
                }
            }

            return cleanedData;
        }

        // --- Data Saving Methods ---
        public void SaveDataToJson(string outputFilePath, List<string> data, string type)
        {
            try
            {
                // Ensure the directory exists
                string directoryPath = Path.GetDirectoryName(outputFilePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Save each sentence as a separate item in the output file
                for (int i = 0; i < data.Count; i++)
                {
                    // Trim leading/trailing whitespace and remove unwanted extra characters
                    string cleanedData = data[i].Trim();

                    // Path for the individual sentence output file (modified to include type)
                    string sentenceOutputPath = Path.Combine(directoryPath, $"{type}sentence{i + 1}.json");

                    // Create the output array with a single sentence
                    var sentenceArray = new List<string> { cleanedData };

                    // Serialize the array to JSON (wraps the sentence in an array)
                    string jsonData = JsonConvert.SerializeObject(sentenceArray, Formatting.Indented);

                    // Write the JSON data directly to the file
                    File.WriteAllText(sentenceOutputPath, jsonData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data to JSON file: {ex.Message}");
            }
        }

    }
}
