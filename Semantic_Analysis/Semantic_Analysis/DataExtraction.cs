
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
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Semantic_Analysis
{
    /// <summary>
    /// The DataExtraction class implements the IDataExtraction interface and provides methods to extract and process data
    /// from various file types such as text, CSV, JSON, XML, PDF, DOCX, etc.
    /// </summary>
    public class DataExtraction : IDataExtraction
    {
        #region Configuration and Directory Methods

        /// <summary>
        /// Loads the configuration from the appsettings.json file.
        /// </summary>
        /// <returns>Configuration object that holds the settings from appsettings.json.</returns>
        public static IConfiguration LoadConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
            configurationBuilder.AddJsonFile("appsettings.json");

            return configurationBuilder.Build();
        }

        /// <summary>
        /// Ensures that the directory specified by the path exists.
        /// If the directory does not exist, it will be created.
        /// </summary>
        /// <param name="directoryPath">The path of the directory to check and create if necessary.</param>
        public static void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        #endregion

        #region Extraction Methods

        /// <summary>
        /// Extracts data from a file based on its extension.
        /// It routes the extraction process to the appropriate method depending on the file type.
        /// </summary>
        /// <param name="filePath">The path of the file to extract data from.</param>
        /// <returns>A list of strings representing the extracted data from the file.</returns>
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
                    case ".docx":  // Handle DOCX files
                        fileContent = ExtractDataFromDocx(filePath);
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

        #endregion

        #region File Type Specific Extraction Methods

        /// <summary>
        /// Extracts data from a plain text (.txt) file.
        /// </summary>
        /// <param name="filePath">The path of the text file to extract data from.</param>
        /// <returns>A list of strings, each representing a line from the text file.</returns>
        public List<string> ExtractDataFromText(string filePath) => File.ReadAllLines(filePath).ToList();

        /// <summary>
        /// Extracts data from a CSV (.csv) file.
        /// </summary>
        /// <param name="filePath">The path of the CSV file to extract data from.</param>
        /// <returns>A list of strings, each representing a line from the CSV file.</returns>
        public List<string> ExtractDataFromCsv(string filePath) => File.ReadAllLines(filePath).ToList();

        /// <summary>
        /// Extracts data from a JSON (.json) file.
        /// </summary>
        /// <param name="filePath">The path of the JSON file to extract data from.</param>
        /// <returns>A list of strings representing the parsed JSON data.</returns>
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
        /// Extracts data from an XML (.xml) file.
        /// </summary>
        /// <param name="filePath">The path of the XML file to extract data from.</param>
        /// <returns>A list of strings representing the XML elements and their values.</returns>
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
        /// Extracts data from a PDF (.pdf) file.
        /// </summary>
        /// <param name="filePath">The path of the PDF file to extract data from.</param>
        /// <returns>A list of strings representing the extracted text from the PDF.</returns>
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
        /// Extracts raw data from an unsupported or binary file type.
        /// </summary>
        /// <param name="filePath">The path of the file to extract raw data from.</param>
        /// <returns>A list of strings representing the first few bytes of the file in hexadecimal format.</returns>
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
        /// Extracts data from a Markdown (.md) file.
        /// </summary>
        /// <param name="filePath">The path of the Markdown file to extract data from.</param>
        /// <returns>A list of strings containing the cleaned text from the Markdown file.</returns>
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
        /// Extracts data from an HTML (.html) file.
        /// </summary>
        /// <param name="filePath">The path of the HTML file to extract data from.</param>
        /// <returns>A list of strings containing the cleaned text from the HTML file.</returns>
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

        /// <summary>
        /// Extracts data from a DOCX (.docx) file.
        /// </summary>
        /// <param name="filePath">The path of the DOCX file to extract data from.</param>
        /// <returns>A list of strings representing the extracted text from the DOCX file.</returns>
        public List<string> ExtractDataFromDocx(string filePath)
        {
            var content = new List<string>();
            try
            {
                // Open the DOCX file
                using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(filePath, false))
                {
                    // Access the main document part
                    var body = wordDoc.MainDocumentPart.Document.Body;

                    // Extract the text from each paragraph
                    foreach (var paragraph in body.Descendants<Paragraph>())
                    {
                        // Extract and add text from the paragraph
                        var paragraphText = string.Join(" ", paragraph.Descendants<Text>().Select(text => text.Text));
                        content.Add(paragraphText);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading DOCX file: {ex.Message}");
                return new List<string> { $"Error: {ex.Message}" };
            }
            return content;
        }
        #endregion

        #region Data Processing Methods

        /// <summary>
        /// Cleans extracted data by removing unwanted characters and normalizing text.
        /// </summary>
        /// <param name="data">The list of strings representing the raw extracted data.</param>
        /// <returns>A cleaned list of strings.</returns>
        public List<string> CleanData(List<string> data)
        {
            // If the input data is null or empty, return an empty list
            if (data == null || data.Count == 0)
            {
                Console.WriteLine("No data to clean.");
                return new List<string>(); // Return an empty list
            }

            var cleanedData = new List<string>();

            foreach (var line in data)
            {
                // Create a new variable to store the modified version of 'line'
                string cleanedLine = line.Replace("\n", " ").Trim();

                // Replace multiple spaces with a single space
                cleanedLine = Regex.Replace(cleanedLine, @"\s+", " ");

                // Split into sentences by punctuation marks (., !, ?)
                var sentences = Regex.Split(cleanedLine, @"(?<=[.!?])\s+");

                foreach (var sentence in sentences)
                {
                    var cleanedSentence = sentence.Trim().ToLower();

                    // Remove leading numbers and period if it's part of a numbered list like "1. Sentence"
                    if (Regex.IsMatch(cleanedSentence, @"^\d+\.\s*"))
                    {
                        cleanedSentence = Regex.Replace(cleanedSentence, @"^\d+\.\s*", "");
                    }

                    // Remove non-alphanumeric characters, preserving spaces, periods, punctuation, and apostrophes
                    cleanedSentence = Regex.Replace(cleanedSentence, @"[^a-zA-Z0-9\s.,!?'-]", "");

                    // Add non-empty cleaned sentence to the list
                    if (!string.IsNullOrEmpty(cleanedSentence))
                        cleanedData.Add(cleanedSentence);
                }
            }

            return cleanedData;
        }

        /// <summary>
        /// Saves the extracted and cleaned data to a JSON file.
        /// </summary>
        /// <param name="outputFilePath">The path where the output JSON file will be saved.</param>
        /// <param name="data">The data to be saved as JSON.</param>
        /// <param name="type">The type of data being saved (e.g., "extracted" or "reference").</param>
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

        #endregion
    }
}
