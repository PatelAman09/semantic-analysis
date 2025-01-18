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
        public static void FileSelection()
        {
            // Initialize OpenFileDialog for file selection
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select a file to extract data from";
            openFileDialog.Filter = "All Files (*.*)|*.*|Text Files (*.txt)|*.txt";  // Filter for multiple file types

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
    }
}
