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

        [TestClass]

        public sealed class TestCase2
        {
            [TestMethod]

            public void CalculateMagnitudeUnitTest()
            {
                double[] vector = { 3.0, 4.0 };

                double result = CosineSimilarity.CalculateMagnitude(vector);

                Assert.AreEqual(5.0, result, "The magnitude should be 5.");
            }

            [TestMethod]

            public void CalculateMagnitude_EmptyVector_ThrowsArgumentException()
            {
                double[] vector = { };

                Assert.ThrowsException<ArgumentException>(() => CosineSimilarity.CalculateMagnitude(vector));
            }

        }

        [TestClass]

        public sealed class TestCase3
        {
            [TestMethod]

            public void cosineSimilarityUnitTest()
            {
                double[] vectorA = { 1.0, 2.0, 3.0 };
                double[] vectorB = { 4.0, 5.0, 6.0 };

                double result = CosineSimilarity.CosineSimilarityCalculation(vectorA, vectorB);

                Assert.AreEqual(0.974631846, result, 1e-6, "Cosine similarity should be approximately 0.9746.");
            }

            [TestMethod]
            public void CosineSimilarity_OneZeroVector_ReturnsZero()
            {
                double[] vectorA = { 0.0, 0.0, 0.0 };
                double[] vectorB = { 4.0, 5.0, 6.0 };

                double result = CosineSimilarity.CosineSimilarityCalculation(vectorA, vectorB);

                Assert.AreEqual(0.0, result, "Cosine similarity between a zero vector and any vector should be 0.");
            }

            [TestMethod]
            public void CosineSimilarity_NullVector_ThrowsArgumentException()
        {
            double[] vectorA = { 1.0, 2.0, 3.0 };
            double[] vectorB = null;

            Assert.ThrowsException<ArgumentException>(() => CosineSimilarity.CosineSimilarityCalculation(vectorA, vectorB));
        }

        }
    }
}
