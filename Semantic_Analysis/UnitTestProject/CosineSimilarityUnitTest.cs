using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Semantic_Analysis;

namespace CosineSimilarity_UnitTest
{
    /// <summary>
    /// Unit tests for the CosineSimilarity class.
    /// </summary>
    public class CosineSimilarityUnitTest
    {
        #region ReadVectorsTests

        /// <summary>
        /// Unit tests for the ReadVectorsFromCsv method.
        /// </summary>
        [TestClass]
        public class ReadVectorsTests
        {
            private CosineSimilarity _cosineSimilarity = null!;

            /// <summary>
            /// Initializes the test instance before each test.
            /// </summary>
            [TestInitialize]
            public void Setup()
            {
                _cosineSimilarity = new CosineSimilarity();
            }

            /// <summary>
            /// Ensures that providing an empty file path throws an ArgumentException.
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void ReadVectorsFromCsv_EmptyFilePath_ThrowsException()
            {
                _cosineSimilarity.ReadVectorsFromCsv("");
            }
        }

        #endregion

        #region ValidateVectorsTests

        /// <summary>
        /// Unit tests for the ValidateVectors method.
        /// </summary>
        [TestClass]
        public class ValidateVectorsTests
        {
            private CosineSimilarity _cosineSimilarity = null!;

            /// <summary>
            /// Initializes the test instance before each test.
            /// </summary>
            [TestInitialize]
            public void Setup()
            {
                _cosineSimilarity = new CosineSimilarity();
            }

            /// <summary>
            /// Ensures that validating an empty vector dictionary throws an InvalidOperationException.
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ValidateVectors_ShouldThrowException_WhenVectorsAreEmpty()
            {
                var emptyVectors = new Dictionary<string, (string text, double[] vector)>();
                _cosineSimilarity.ValidateVectors(emptyVectors);
            }

            /// <summary>
            /// Ensures that validating vectors with different lengths throws an InvalidOperationException.
            /// </summary>
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

        #endregion

        #region CosineSimilarityCalculationTests

        /// <summary>
        /// Unit tests for the CosineSimilarityCalculation method.
        /// </summary>
        [TestClass]
        public class CosineSimilarityCalculationTests
        {
            private CosineSimilarity _cosineSimilarity = null!;

            /// <summary>
            /// Initializes the test instance before each test.
            /// </summary>
            [TestInitialize]
            public void Setup()
            {
                _cosineSimilarity = new CosineSimilarity();
            }

            /// <summary>
            /// Ensures that the cosine similarity calculation returns the correct value for valid vectors.
            /// </summary>
            [TestMethod]
            public void CosineSimilarityCalculation_ValidVectors_ReturnsCorrectValue()
            {
                double[] vectorA = { 1.0, 2.0, 3.0 };
                double[] vectorB = { 4.0, 5.0, 6.0 };

                double similarity = _cosineSimilarity.CosineSimilarityCalculation(vectorA, vectorB);

                double expected = 0.974631846;
                Assert.AreEqual(expected, similarity, 0.0001);
            }

            /// <summary>
            /// Ensures that the cosine similarity calculation returns zero when one vector has zero magnitude.
            /// </summary>
            [TestMethod]
            public void CosineSimilarityCalculation_ZeroMagnitudeVector_ReturnsZero()
            {
                double[] vectorA = { 0.0, 0.0, 0.0 };
                double[] vectorB = { 4.0, 5.0, 6.0 };

                double similarity = _cosineSimilarity.CosineSimilarityCalculation(vectorA, vectorB);

                Assert.AreEqual(0.0, similarity);
            }
        }

        #endregion

        #region SaveOutputTests

        /// <summary>
        /// Unit tests for the SaveOutputToCsv method.
        /// </summary>
        [TestClass]
        public class SaveOutputTests
        {
            private CosineSimilarity _cosineSimilarity = null!;

            /// <summary>
            /// Initializes the test instance before each test.
            /// </summary>
            [TestInitialize]
            public void Setup()
            {
                _cosineSimilarity = new CosineSimilarity();
            }

            /// <summary>
            /// Ensures that the SaveOutputToCsv method correctly saves output to a CSV file.
            /// </summary>
            [TestMethod]
            public void SaveOutputToCsv_ValidData_SavesFileSuccessfully()
            {
                string testOutputPath = Path.GetTempFileName();
                List<string> outputData = new List<string> { "Test Cosine Similarity Output" };

                _cosineSimilarity.SaveOutputToCsv(testOutputPath, outputData);
                var savedData = File.ReadAllLines(testOutputPath);

                Assert.AreEqual(outputData[0], savedData[0]);
            }

            /// <summary>
            /// Ensures that SaveOutputToCsv creates a file with the correct content.
            /// </summary>
            [TestMethod]
            public void SaveOutputToCsv_ShouldCreateFileWithCorrectContent()
            {
                string testFilePath = Path.Combine(Path.GetTempPath(), "test_output.csv");
                var outputData = new List<string> { "Word1,Word2,Cosine Similarity", "word1,word2,0.95" };

                _cosineSimilarity.SaveOutputToCsv(testFilePath, outputData);

                Assert.IsTrue(File.Exists(testFilePath));
                var lines = File.ReadAllLines(testFilePath);
                CollectionAssert.AreEqual(outputData, lines);

                File.Delete(testFilePath);
            }
        }

        #endregion
    }
}
