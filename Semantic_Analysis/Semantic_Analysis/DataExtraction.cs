using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
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

            // Retrieve folder paths from configuration
            string dataPreprocessingPath = configuration["FilePaths:DataPreprocessing"];
            string extractedDataPath = configuration["FilePaths:ExtractedData"]; // Now using ExtractedData folder for both

            // Manually retrieving supported extensions from the configuration
            var supportedExtensions = configuration.GetSection("FilePaths:SupportedFileExtensions")
                                                     .AsEnumerable()       // Get all key-value pairs
                                                     .Select(x => x.Value) // Select the values (file extensions)
                                                     .ToList();

            // Resolve the absolute paths for the directories
            string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            string absoluteDataPreprocessingPath = Path.Combine(projectRoot, dataPreprocessingPath);
            string absoluteExtractedDataPath = Path.Combine(projectRoot, extractedDataPath); // Using the ExtractedData folder

            // Ensure the necessary directories exist
            EnsureDirectoryExists(absoluteExtractedDataPath);

            // Get all files in the RawData folder with supported extensions
            var filesInRawData = Directory.GetFiles(absoluteDataPreprocessingPath)
                                          .Where(file => supportedExtensions.Contains(Path.GetExtension(file).ToLower()))
                                          .ToList();

            // Ensure exactly two files are found (1 extracted data and 1 reference document)
            if (filesInRawData.Count != 2)
            {
                Console.WriteLine("Error: There should be exactly two files in the RawData folder.");
                return;
            }

            // Treat the first file as extracted data and the second as reference document
            string extractedDataFilePath = filesInRawData[0];
            string referenceDocumentFilePath = filesInRawData[1];

            // Define the output file paths for both processed data in the ExtractedData folder
            string outputExtractedDataFilePath = Path.Combine(absoluteExtractedDataPath, $"{Path.GetFileNameWithoutExtension(extractedDataFilePath)}.json");
            string outputReferenceDocumentFilePath = Path.Combine(absoluteExtractedDataPath, $"{Path.GetFileNameWithoutExtension(referenceDocumentFilePath)}.json");

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
                    // Clean sentence: Trim spaces, convert to lowercase, and remove special characters
                    var cleanedSentence = Regex.Replace(sentence
                            .Trim()  // Remove leading/trailing spaces
                            .ToLower(), // Convert to lowercase
                            @"[^a-zA-Z0-9\s]", "");// Remove all non-alphanumeric characters (except spaces)

                    // Add non-empty cleaned sentence to the list
                    if (!string.IsNullOrEmpty(cleanedSentence))
                        cleanedData.Add(cleanedSentence);
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

                // Collect all cleaned sentences in a list
                List<string> allSentences = new List<string>();

                // Add each sentence to the list
                foreach (var sentence in data)
                {
                    // Clean up the sentence by trimming whitespace
                    string cleanedData = sentence.Trim();

                    // Add the cleaned sentence to the list
                    if (!string.IsNullOrEmpty(cleanedData))
                    {
                        allSentences.Add(cleanedData);
                    }
                }

                // Serialize the list of sentences to JSON
                string jsonData = JsonConvert.SerializeObject(allSentences, Formatting.Indented);

                // Write the JSON data to the output file
                File.WriteAllText(outputFilePath, jsonData);

                Console.WriteLine($"Data saved to JSON file: {outputFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data to JSON file: {ex.Message}");
            }
        }
    }
}
