using System;
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
