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

        Dictionary<string, double[]> ReadVectorsFromCsv(string inputFilePath);
        void ValidateVectors(Dictionary<string, double[]> vectors);
        double CosineSimilarityCalculation(double[] vectorA, double[] vectorB);
        void SaveOutputToCsv(string outputFilePath, List<string> outputData);
    }
}
