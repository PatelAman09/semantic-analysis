using System;
using System.Collections.Generic;

namespace Semantic_Analysis
{
    public interface IDataExtraction
    {
        List<string> ExtractDataFromFile(string filePath);
        List<string> ExtractDataFromText(string filePath);
        List<string> ExtractDataFromCsv(string filePath);
        List<string> ExtractDataFromJson(string filePath);
        List<string> ExtractDataFromXml(string filePath);
        List<string> ExtractDataFromPdf(string filePath);
        List<string> ExtractDataFromMarkdown(string filePath);
        List<string> ExtractDataFromHtml(string filePath);
        List<string> ExtractRawData(string filePath);

        List<string> CleanData(List<string> data);
        void SaveDataToText(string outputFilePath, List<string> data);
    }
}
