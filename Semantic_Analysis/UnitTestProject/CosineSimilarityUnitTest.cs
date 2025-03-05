<<<<<<< HEAD
﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Semantic_Analysis;

//namespace UnitTestProject
//{
//    public class CosineSimilarityUnitTest
//    {
//        [TestClass]
//        public sealed class TestCase1
//        {
//            [TestMethod]

//            public void CalculateDotProductUnitTest()
//            {

//                double[] vectorA = { 1.0, 2.0, 3.0 };
//                double[] vectorB = { 4.0, 5.0, 6.0 };
//                double result = CosineSimilarity.CalculateDotProduct(vectorA, vectorB);
//                Assert.AreEqual(32.0, result, "The dot product should be 32.");

//            }
//        }

//        [TestClass]

//        public sealed class TestCase2
//        {
//            [TestMethod]

//            public void CalculateMagnitudeUnitTest()
//            {
//                double[] vector = { 3.0, 4.0 };

//                double result = CosineSimilarity.CalculateMagnitude(vector);

//                Assert.AreEqual(5.0, result, "The magnitude should be 5.");
//            }

//            [TestMethod]

//            public void CalculateMagnitude_EmptyVector_ThrowsArgumentException()
//            {
//                double[] vector = { };

//                Assert.ThrowsException<ArgumentException>(() => CosineSimilarity.CalculateMagnitude(vector));
//            }

//        }

//        [TestClass]

//        public sealed class TestCase3
//        {
//            [TestMethod]

//            public void cosineSimilarityUnitTest()
//            {
//                double[] vectorA = { 1.0, 2.0, 3.0 };
//                double[] vectorB = { 4.0, 5.0, 6.0 };

//                double result = CosineSimilarity.CosineSimilarityCalculation(vectorA, vectorB);

//                Assert.AreEqual(0.974631846, result, 1e-6, "Cosine similarity should be approximately 0.9746.");
//            }

//            [TestMethod]
//            public void CosineSimilarity_OneZeroVector_ReturnsZero()
//            {
//                double[] vectorA = { 0.0, 0.0, 0.0 };
//                double[] vectorB = { 4.0, 5.0, 6.0 };

//                double result = CosineSimilarity.CosineSimilarityCalculation(vectorA, vectorB);

//                Assert.AreEqual(0.0, result, "Cosine similarity between a zero vector and any vector should be 0.");
//            }

//            [TestMethod]
//            public void CosineSimilarity_NullVector_ThrowsArgumentException()
//        {
//            double[] vectorA = { 1.0, 2.0, 3.0 };
//            double[] vectorB = null;

//            Assert.ThrowsException<ArgumentException>(() => CosineSimilarity.CosineSimilarityCalculation(vectorA, vectorB));
//        }

//        }
//    }
//}
=======
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Semantic_Analysis;
using Semantic_Analysis.Interfaces;

namespace UnitTestProject
{
    public class CosineSimilarityUnitTest
    {
        [TestClass]
        public class ReadVectorsTests
        {
            private CosineSimilarity _cosineSimilarity = null!;

            [TestInitialize]
            public void Setup()
            {
                _cosineSimilarity = new CosineSimilarity();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void ReadVectorsFromCsv_EmptyFilePath_ThrowsException()
            {
                _cosineSimilarity.ReadVectorsFromCsv("");
            }
        }

        [TestClass]
        public class ValidateVectorsTests
        {
            private CosineSimilarity _cosineSimilarity = null!;

            [TestInitialize]
            public void Setup()
            {
                _cosineSimilarity = new CosineSimilarity();
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ValidateVectors_IncorrectVectorLengths_ThrowsException()
            {
                var vectors = new Dictionary<string, double[]>()
            {
                { "Vector1", new double[] { 1.0, 2.0 } },
                { "Vector2", new double[] { 3.0, 4.0, 5.0 } }
            };

                _cosineSimilarity.ValidateVectors(vectors);
            }
        }

        [TestClass]
        public class CosineSimilarityCalculationTests
        {
            private CosineSimilarity _cosineSimilarity = null!;

            [TestInitialize]
            public void Setup()
            {
                _cosineSimilarity = new CosineSimilarity();
            }

            [TestMethod]
            public void CosineSimilarityCalculation_ValidVectors_ReturnsCorrectValue()
            {
                // Arrange
                double[] vectorA = { 1.0, 2.0, 3.0 };
                double[] vectorB = { 4.0, 5.0, 6.0 };

                // Act
                double similarity = _cosineSimilarity.CosineSimilarityCalculation(vectorA, vectorB);

                // Assert
                double expected = 0.974631846;
                Assert.AreEqual(expected, similarity, 0.0001);
            }

            [TestMethod]
            public void CosineSimilarityCalculation_ZeroMagnitudeVector_ReturnsZero()
            {
                // Arrange
                double[] vectorA = { 0.0, 0.0, 0.0 };
                double[] vectorB = { 4.0, 5.0, 6.0 };

                // Act
                double similarity = _cosineSimilarity.CosineSimilarityCalculation(vectorA, vectorB);

                // Assert
                Assert.AreEqual(0.0, similarity);
            }
        }

        [TestClass]
        public class SaveOutputTests
        {
            private CosineSimilarity _cosineSimilarity = null!;

            [TestInitialize]
            public void Setup()
            {
                _cosineSimilarity = new CosineSimilarity();
            }

            [TestMethod]
            public void SaveOutputToCsv_ValidData_SavesFileSuccessfully()
            {
                // Arrange
                string testOutputPath = Path.GetTempFileName();
                List<string> outputData = new List<string>
            {
                "Test Cosine Similarity Output"
            };

                // Act
                _cosineSimilarity.SaveOutputToCsv(testOutputPath, outputData);
                var savedData = File.ReadAllLines(testOutputPath);

                // Assert
                Assert.AreEqual(outputData[0], savedData[0]);
            }
        }
    }
}
>>>>>>> origin/Development
