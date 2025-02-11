using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semantic_Analysis.Interfaces
{
    public interface ICosineSimilarity
    {

        List<double[]> ReadVectorsFromCsv(string inputFilePath);
        void ValidateVectors(List<double[]> vectors);
        double CalculateDotProduct(double[] vectorA, double[] vectorB);
        double CalculateMagnitude(double[] vector);
        double CosineSimilarityCalculation(double[] vectorA, double[] vectorB);
        void SaveOutputToCsv(string outputFilePath, List<string> data);
    }
}
