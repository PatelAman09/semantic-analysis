using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Semantic_Analysis
{
    /// This class provides methods for extracting data from various file types 
    public class DataExtraction
    {
        // --- Data Extraction Methods ---
        /// Extracts data from a file based on its type. 
        /// <param name="filePath">The full path of the PDF file to extract data from.</param>
        /// <returns>A list of strings representing the extracted text from each page of the PDF.</returns>
        public List<string> ExtractDataFromFile(string filePath)
        {
            var fileContent = new List<string>();
            try
            {
                string fileExtension = Path.GetExtension(filePath);

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
