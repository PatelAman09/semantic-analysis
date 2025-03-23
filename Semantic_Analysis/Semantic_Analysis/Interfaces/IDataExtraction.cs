using System;
using System.Collections.Generic;

namespace Semantic_Analysis
{
    /// <summary>
    /// Interface defining methods for extracting and processing data from various file types.
    /// </summary>
    public interface IDataExtraction
    {
        /// <summary>
        /// Extracts data from a file based on its extension (e.g., .txt, .csv, .json, etc.).
        /// </summary>
        /// <param name="filePath">The path to the file from which data is to be extracted.</param>
        /// <returns>A list of strings representing the extracted data.</returns>
        List<string> ExtractDataFromFile(string filePath);

        /// <summary>
        /// Extracts data from a text file (.txt).
        /// </summary>
        /// <param name="filePath">The path to the .txt file from which data is to be extracted.</param>
        /// <returns>A list of strings representing the extracted data from the text file.</returns>
        List<string> ExtractDataFromText(string filePath);

        /// <summary>
        /// Extracts data from a CSV file (.csv).
        /// </summary>
        /// <param name="filePath">The path to the .csv file from which data is to be extracted.</param>
        /// <returns>A list of strings representing the extracted data from the CSV file.</returns>
        List<string> ExtractDataFromCsv(string filePath);

        /// <summary>
        /// Extracts data from a JSON file (.json).
        /// </summary>
        /// <param name="filePath">The path to the .json file from which data is to be extracted.</param>
        /// <returns>A list of strings representing the extracted data from the JSON file.</returns>
        List<string> ExtractDataFromJson(string filePath);

        /// <summary>
        /// Extracts data from an XML file (.xml).
        /// </summary>
        /// <param name="filePath">The path to the .xml file from which data is to be extracted.</param>
        /// <returns>A list of strings representing the extracted data from the XML file.</returns>
        List<string> ExtractDataFromXml(string filePath);

        /// <summary>
        /// Extracts data from a PDF file (.pdf).
        /// </summary>
        /// <param name="filePath">The path to the .pdf file from which data is to be extracted.</param>
        /// <returns>A list of strings representing the extracted data from the PDF file.</returns>
        List<string> ExtractDataFromPdf(string filePath);

        /// <summary>
        /// Extracts data from a Markdown file (.md).
        /// </summary>
        /// <param name="filePath">The path to the .md file from which data is to be extracted.</param>
        /// <returns>A list of strings representing the extracted data from the Markdown file.</returns>
        List<string> ExtractDataFromMarkdown(string filePath);

        /// <summary>
        /// Extracts data from an HTML file (.html, .htm).
        /// </summary>
        /// <param name="filePath">The path to the .html or .htm file from which data is to be extracted.</param>
        /// <returns>A list of strings representing the extracted data from the HTML file.</returns>
        List<string> ExtractDataFromHtml(string filePath);

        /// <summary>
        /// Extracts data from a DOCX file (.docx).
        /// </summary>
        /// <param name="filePath">The path to the .docx file from which data is to be extracted.</param>
        /// <returns>A list of strings representing the extracted data from the DOCX file.</returns>
        List<string> ExtractDataFromDocx(string filePath);

        /// <summary>
        /// Extracts raw data from an unknown or binary file.
        /// </summary>
        /// <param name="filePath">The path to the unknown or binary file from which raw data is to be extracted.</param>
        /// <returns>A list of strings representing the raw data extracted from the file.</returns>
        List<string> ExtractRawData(string filePath);

        /// <summary>
        /// Cleans extracted data by removing special characters, converting to lowercase, trimming whitespace, etc.
        /// </summary>
        /// <param name="data">A list of strings representing the raw extracted data to be cleaned.</param>
        /// <returns>A cleaned list of strings.</returns>
        List<string> CleanData(List<string> data);

        /// <summary>
        /// Saves the cleaned data to a JSON file, with the output file named based on the data type.
        /// </summary>
        /// <param name="outputFilePath">The path where the cleaned data JSON file will be saved.</param>
        /// <param name="data">A list of strings representing the cleaned data.</param>
        /// <param name="type">A string representing the type of the data being saved (e.g., "text", "csv", etc.).</param>
        void SaveDataToJson(string outputFilePath, List<string> data, string type);
    }
}
