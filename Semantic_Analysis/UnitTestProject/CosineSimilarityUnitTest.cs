using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Semantic_Analysis;
using Semantic_Analysis.Interfaces;

namespace CosineSimilarity_UnitTest
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
            public void ValidateVectors_ShouldThrowException_WhenVectorsAreEmpty()
            {
                var emptyVectors = new Dictionary<string, (string text, double[] vector)>();
                _cosineSimilarity.ValidateVectors(emptyVectors);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ValidateVectors_ShouldThrowException_WhenVectorsHaveDifferentLengths()
            {
                var vectors = new Dictionary<string, (string, double[])>
            {
                { "1", ("text1", new double[] { 1.0, 2.0 }) },
                { "2", ("text2", new double[] { 1.0, 2.0, 3.0 }) }
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
            [TestMethod]
            public void SaveOutputToCsv_ShouldCreateFileWithCorrectContent()
            {
                string testFilePath = Path.Combine(Path.GetTempPath(), "test_output.csv");
                var outputData = new List<string> { "Index1,Index2,Word1,Word2,Cosine Similarity", "1,2,word1,word2,0.95" };

                _cosineSimilarity.SaveOutputToCsv(testFilePath, outputData);

                Assert.IsTrue(File.Exists(testFilePath));
                var lines = File.ReadAllLines(testFilePath);
                CollectionAssert.AreEqual(outputData, lines);

                File.Delete(testFilePath);
            }
        }
    }
}
