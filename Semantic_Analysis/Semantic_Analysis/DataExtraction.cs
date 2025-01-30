using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.Windows.Forms;


namespace Semantic_Analysis
{
    
    /// This class provides methods for extracting data from various file types 
    public class DataExtraction
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // Call FileSelection directly when the application starts
            FileSelection();
        }

        public static void FileSelection()
        {
            // Initialize OpenFileDialog for file selection
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select a file to extract data from";
            openFileDialog.Filter = "All Files (*.*)|*.*|Text Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv";  // Filter for multiple file types

            // Show dialog and check if the user selects a file
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string userFilePath = openFileDialog.FileName;  // Get the selected file path

                // Check if the file exists (should always be true after file dialog)
                if (File.Exists(userFilePath))
                {
                    // Create an instance of the DataProcessor class
                    DataExtraction processor = new DataExtraction();

                    // Extract data from the file
                    List<string> extractedData = processor.ExtractDataFromFile(userFilePath);

                    // Clean the extracted data
                    extractedData = processor.CleanData(extractedData);  

                    // Initialize SaveFileDialog to ask the user where to save the output JSON file
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Title = "Save Extracted Data as JSON";
                    saveFileDialog.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
                    saveFileDialog.FileName = Path.GetFileNameWithoutExtension(userFilePath) + "_extracted.json"; // Default name

                    // Show dialog and check if the user selects a location and file name
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string outputFilePath = saveFileDialog.FileName;

                        // Save the extracted data to the JSON file
                        processor.SaveDataToJson(outputFilePath, extractedData);

                        Console.WriteLine($"\nData extracted and saved to: {outputFilePath}");
                    }
                    else
                    {
                        Console.WriteLine("No location or file name was selected. Exiting the program.");
                    }
                }
                else
                {
                    Console.WriteLine("The selected file does not exist. Please try again.");
                }
            }
            else
            {
                Console.WriteLine("No file was selected. Exiting the program.");
            }
        }

        // --- Data Extraction Methods ---

        
        /// Extracts data from a file based on its type. 
        /// <param name="filePath">The full path of the PDF file to extract data from.</param>
        /// <returns>A list of strings representing the extracted text from each page of the PDF.</returns>
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
                    default:
                        fileContent = ExtractRawData(filePath);
                        break;


                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error reading file {filePath}: {ex.Message}");
            }
            return fileContent;
        }


        /// Extracts text from a plain text file. <summary>
        /// <param name="filePath">The full path of the text file to extract data from.</param>
        /// <returns>A list of strings representing the lines of text in the file.</returns>
        private List<string> ExtractDataFromText(string filePath)
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


        /// Extracts data from a CSV file.
        /// <param name="filePath">The path to the CSV file.</param>
        /// <returns>A list of strings containing the extracted CSV data.</returns>
        private List<string> ExtractDataFromCsv(string filePath)
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

     
        /// Extracts data from a JSON file and returns key-value pairs.
        /// <param name="filePath">The path to the JSON file.</param>
        /// <returns>A list of strings containing key-value pairs from the JSON file.</returns>
        private List<string> ExtractDataFromJson(string filePath)
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

        /// Extracts data from an XML file.
        /// <param name="filePath">The path to the XML file.</param>
        /// <returns>A list of strings containing extracted XML data.</returns>
        private List<string> ExtractDataFromXml(string filePath)
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

        /// Extracts text data from an HTML file by removing HTML tags.
        /// <param name="filePath">The path to the HTML file.</param>
        /// <returns>A list of strings containing the extracted text data.</returns>
        private List<string> ExtractDataFromHtml(string filePath)
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


        /// Extracts text data from a Markdown file by removing Markdown syntax.
        /// <param name="filePath">The path to the Markdown file.</param>
        /// <returns>A list of strings containing the extracted text data.</returns>
        private List<string> ExtractDataFromMarkdown(string filePath)
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


        /// Extracts raw byte data from a file.
        /// <param name="filePath">The path to the file.</param>
        /// <returns>A list containing a representation of the raw bytes.</returns>
        private List<string> ExtractRawData(string filePath)
        {
            var data = new List<string>();
            try
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                string rawContent = BitConverter.ToString(bytes.Take(100).ToArray()); // Get first 100 bytes
                data.Add($"Raw Content (first 100 bytes): {rawContent}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading raw file: {ex.Message}");
                data.Add($"Error: {ex.Message}");
            }
            return data;
        }



        public void SaveDataToJson(string outputFilePath, List<string> data)
        {
            try
            {
                var jsonContent = JsonConvert.SerializeObject(new { extractedData = data }, Formatting.Indented);
                File.WriteAllText(outputFilePath, jsonContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data to JSON file: {ex.Message}");
            }
        }

        /// Cleans extracted data by trimming whitespace, removing special characters, and converting to lowercase.
        /// <param name="data">The extracted data to be cleaned.</param>
        /// <returns>A list of cleaned strings.</returns>
        private List<string> CleanData(List<string> data)
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

    }
}
