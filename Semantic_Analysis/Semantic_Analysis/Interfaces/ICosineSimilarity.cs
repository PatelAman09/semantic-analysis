using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semantic_Analysis.Interfaces
{
    public interface ICosineSimilarity
    {
        Dictionary<string, (string text, double[] vector)> ReadVectorsFromCsv(string inputFilePath);
        double[] NormalizeVector(double[] vector);
        void ValidateVectors(Dictionary<string, (string text, double[] vector)> vectors);
        double CosineSimilarityCalculation(double[] vectorA, double[] vectorB);
        void SaveOutputToCsv(string outputFilePath, List<string> outputData);
        double CalculateDocumentSimilarity(Dictionary<string, (string text, double[] vector)> vectorsFile1, Dictionary<string, (string text, double[] vector)> vectorsFile2);
    }
}
