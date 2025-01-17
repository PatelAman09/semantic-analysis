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
        public static void Fileselection()
        {
            try
            {
                // Initialize OpenFileDialog for file selection
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Title = "Select a file to extract data from",
                    Filter = "All Files (*.*)|*.*|Text Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv|JSON Files (*.json)|*.json|XML Files (*.xml)|*.xml|HTML Files (*.html)|*.html|Markdown Files (*.md)|*.md|PDF Files (*.pdf)|*.pdf"
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string userFilePath = openFileDialog.FileName;
                    if (File.Exists(userFilePath))
                    {
                        // Extract data from the selected file
                        DataExtraction processor = new DataExtraction();
                        List<string> extractedData = processor.ExtractDataFromFile(userFilePath);

                        // Display the extracted data
                        Console.WriteLine("\nExtracted Data:");
                        foreach (var line in extractedData)
                        {
                            Console.WriteLine(line);
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
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
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
    }
}
