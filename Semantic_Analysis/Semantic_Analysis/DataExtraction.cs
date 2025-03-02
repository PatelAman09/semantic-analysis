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
    /// <summary>
    /// Class responsible for extracting, cleaning, and saving data from various file types.
    /// </summary>
    public class DataExtraction : IDataExtraction
    {
        /// <summary>
        /// Main entry point of the application, which coordinates the file extraction and saving process.
        /// </summary>
        /// <param name="args">Command-line arguments (not used in this implementation).</param>
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
            processor.SaveDataToJson(outputExtractedDataFilePath, extractedData);

            // Process the reference document file
            List<string> referenceData = processor.ExtractDataFromFile(referenceDocumentFilePath);
            referenceData = processor.CleanData(referenceData);
            processor.SaveDataToJson(outputReferenceDocumentFilePath, referenceData);

            // Output the result of the data extraction
            Console.WriteLine($"Data extracted and saved to: {outputExtractedDataFilePath}");
            Console.WriteLine($"Reference document data extracted and saved to: {outputReferenceDocumentFilePath}");
        }

        /// <summary>
        /// Loads configuration from the appsettings.json file.
        /// </summary>
        /// <returns>The loaded configuration.</returns>
        private static IConfiguration LoadConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
            configurationBuilder.AddJsonFile("appsettings.json");

            return configurationBuilder.Build();
        }

        /// <summary>
        /// Ensures that the specified directory exists. If not, it creates the directory.
        /// </summary>
        /// <param name="directoryPath">The directory path to ensure exists.</param>
        private static void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        /// <summary>
        /// Finds a file within a directory based on a list of filenames.
        /// </summary>
        /// <param name="directoryPath">The directory in which to search.</param>
        /// <param name="fileNames">A list of filenames to search for.</param>
        /// <returns>The full path of the first matching file, or null if no match is found.</returns>
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

        /// <summary>
        /// Extracts data from various types of files based on their extensions.
        /// </summary>
        /// <param name="filePath">The path of the file to extract data from.</param>
        /// <returns>A list of strings containing the extracted data.</returns>
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

        /// <summary>
        /// Extracts data from a plain text file.
        /// </summary>
        public List<string> ExtractDataFromText(string filePath) => File.ReadAllLines(filePath).ToList();

        /// <summary>
        /// Extracts data from a CSV file.
        /// </summary>
        public List<string> ExtractDataFromCsv(string filePath) => File.ReadAllLines(filePath).ToList();

        /// <summary>
        /// Extracts data from a JSON file.
        /// </summary>
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

        /// <summary>
        /// Extracts data from an XML file.
        /// </summary>
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

        /// <summary>
        /// Extracts text data from a PDF file.
        /// </summary>
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

        /// <summary>
        /// Extracts raw byte data from a file.
        /// </summary>
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

        /// <summary>
        /// Extracts text data from a Markdown file.
        /// </summary>
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

        /// <summary>
        /// Extracts text data from an HTML file by removing HTML tags.
        /// </summary>
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

        /// <summary>
        /// Cleans extracted data by trimming whitespace, removing special characters, and converting text to lowercase.
        /// </summary>
        public List<string> CleanData(List<string> data)
        {
            return data
                .Select(line => line.Trim())
                .Select(line => Regex.Replace(line, @"[^A-Za-z0-9\s]", "")) // Remove special characters
                .Select(line => line.ToLower()) // Convert to lowercase
                .Where(line => !string.IsNullOrEmpty(line)) // Remove empty lines
                .ToList();
        }

        // --- Data Saving Methods ---

        /// <summary>
        /// Saves the cleaned data to a JSON file.
        /// </summary>
        public void SaveDataToJson(string outputFilePath, List<string> data)
        {
            try
            {
                string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(outputFilePath, jsonData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data to JSON file: {ex.Message}");
            }
        }
    }
}
