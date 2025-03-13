using System;
using System.Collections.Generic;

namespace Semantic_Analysis
{
    public interface IDataExtraction
    {
        // Method to extract data from a file based on its extension
        List<string> ExtractDataFromFile(string filePath);

        // Method to extract data from a text file (.txt)
        List<string> ExtractDataFromText(string filePath);

        // Method to extract data from a CSV file (.csv)
        List<string> ExtractDataFromCsv(string filePath);

        // Method to extract data from a JSON file (.json)
        List<string> ExtractDataFromJson(string filePath);

        // Method to extract data from an XML file (.xml)
        List<string> ExtractDataFromXml(string filePath);

        // Method to extract data from a PDF file (.pdf)
        List<string> ExtractDataFromPdf(string filePath);

        // Method to extract data from a Markdown file (.md)
        List<string> ExtractDataFromMarkdown(string filePath);

        // Method to extract data from an HTML file (.html, .htm)
        List<string> ExtractDataFromHtml(string filePath);

        // Method to extract data from a DOCX file (.docx)
        List<string> ExtractDataFromDocx(string filePath);

        // Method to extract raw data from an unknown or binary file
        List<string> ExtractRawData(string filePath);

        // Method to clean extracted data (e.g., remove special characters, convert to lowercase, trim whitespace)
        List<string> CleanData(List<string> data);

        // Method to save the cleaned data to a JSON file with specific file names based on the data type
        void SaveDataToJson(string outputFilePath, List<string> data, string type);
    }
}
