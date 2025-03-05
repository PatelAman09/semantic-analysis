using System;
using System.Collections.Generic;

namespace Semantic_Analysis
{
    public interface IDataExtraction
    {
        // Method to extract data from a file based on its type
        List<string> ExtractDataFromFile(string filePath);

        // Method to extract data from a text file
        List<string> ExtractDataFromText(string filePath);

        // Method to extract data from a CSV file
        List<string> ExtractDataFromCsv(string filePath);

        // Method to extract data from a JSON file
        List<string> ExtractDataFromJson(string filePath);

        // Method to extract data from an XML file
        List<string> ExtractDataFromXml(string filePath);

        // Method to extract data from a PDF file
        List<string> ExtractDataFromPdf(string filePath);

        // Method to extract data from a Markdown file
        List<string> ExtractDataFromMarkdown(string filePath);

        // Method to extract data from an HTML file
        List<string> ExtractDataFromHtml(string filePath);

        // Method to extract raw data from a file
        List<string> ExtractRawData(string filePath);

        // Method to clean extracted data (e.g., remove special characters, convert to lowercase, trim whitespace)
        List<string> CleanData(List<string> data);

        // Method to save data to a JSON file with specific file names based on the data type
        void SaveDataToJson(string outputFilePath, List<string> data, string type);
    }
}
