using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Semantic_Analysis.Interfaces
{
    public interface IEmbeddingProcessor
    {
        Task<string> ReadJsonFileAsync(string jsonFilePath);
        List<string> AnalyzeJson(string jsonContent);
        Task GenerateAndSaveEmbeddingsAsync(string apiKey, List<string> descriptions, string csvFilePath, int saveInterval);
        Task ProcessJsonFileAsync(string jsonFilePath, string csvFilePath, string apiKey, int saveInterval);
    }
}
