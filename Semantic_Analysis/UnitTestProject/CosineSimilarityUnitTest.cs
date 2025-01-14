using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Semantic_Analysis;

namespace UnitTestProject
{
    public class CosineSimilarityUnitTest
    {
        [TestClass]
        public sealed class TestCase1
        {
            [TestMethod]

            public void CalculateDotProductUnitTest()
            {

                double[] vectorA = { 1.0, 2.0, 3.0 };
                double[] vectorB = { 4.0, 5.0, 6.0 };
                double result = CosineSimilarity.CalculateDotProduct(vectorA, vectorB);
                Assert.AreEqual(32.0, result, "The dot product should be 32.");

            }
        }
    }
}
