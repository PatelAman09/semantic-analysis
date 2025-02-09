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
        public class DotProductTests
        {
            private CosineSimilarity _cosineSimilarity = null!;

            [TestInitialize]
            public void Setup()
            {
                _cosineSimilarity = new CosineSimilarity();
            }

            [TestMethod]
            public void CalculateDotProduct_ValidVectors_ReturnsCorrectResult()
            {
                double[] vectorA = { 1, 2, 3 };
                double[] vectorB = { 4, 5, 6 };
                double expected = 32;

                double result = _cosineSimilarity.CalculateDotProduct(vectorA, vectorB);

                Assert.AreEqual(expected, result);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void CalculateDotProduct_DifferentLengths_ThrowsArgumentException()
            {
                double[] vectorA = { 1, 2 };
                double[] vectorB = { 1, 2, 3 };

                _cosineSimilarity.CalculateDotProduct(vectorA, vectorB);
            }
        }

        [TestClass]
        public class MagnitudeTests
        {
            private CosineSimilarity _cosineSimilarity = null!;

            [TestInitialize]
            public void Setup()
            {
                _cosineSimilarity = new CosineSimilarity();
            }

            [TestMethod]
            public void CalculateMagnitude_ValidVector_ReturnsCorrectResult()
            {
                double[] vector = { 3, 4 };
                double expected = 5;

                double result = _cosineSimilarity.CalculateMagnitude(vector);

                Assert.AreEqual(expected, result);
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
            public void CosineSimilarityCalculation_ValidVectors_ReturnsCorrectSimilarity()
            {
                double[] vectorA = { 1, 0, -1 };
                double[] vectorB = { -1, 0, 1 };
                double expected = -1.0;

                double result = _cosineSimilarity.CosineSimilarityCalculation(vectorA, vectorB);

                Assert.AreEqual(expected, result, 10);
            }

            [TestMethod]
            public void CosineSimilarityCalculation_ZeroMagnitude_ReturnsZero()
            {
                double[] vectorA = { 0, 0, 0 };
                double[] vectorB = { 1, 2, 3 };

                double result = _cosineSimilarity.CosineSimilarityCalculation(vectorA, vectorB);

                Assert.AreEqual(0.0, result);
            }

            [TestMethod]
            public void CosineSimilarityCalculation_IdenticalVectors_ReturnsOne()
            {
                double[] vectorA = { 1, 2, 3 };
                double[] vectorB = { 1, 2, 3 };
                double expected = 1.0;

                double result = _cosineSimilarity.CosineSimilarityCalculation(vectorA, vectorB);

                Assert.AreEqual(expected, result, 10);
            }
        }

        [TestClass]
        public class VectorValidationTests
        {
            private CosineSimilarity _cosineSimilarity = null!;

            [TestInitialize]
            public void Setup()
            {
                _cosineSimilarity = new CosineSimilarity();
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ValidateVectors_InconsistentLengths_ThrowsException()
            {
                List<double[]> vectors = new List<double[]> {
                new double[] {1, 2, 3},
                new double[] {4, 5}
            };

                _cosineSimilarity.ValidateVectors(vectors);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ValidateVectors_LessThanTwoVectors_ThrowsException()
            {
                List<double[]> vectors = new List<double[]> { new double[] { 1, 2, 3 } };

                _cosineSimilarity.ValidateVectors(vectors);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ValidateVectors_EmptyList_ThrowsException()
            {
                List<double[]> vectors = new List<double[]>();
                _cosineSimilarity.ValidateVectors(vectors);
            }
        }

        [TestClass]
        public class CsvHandlingTests
        {
            private CosineSimilarity _cosineSimilarity = null!;

            [TestInitialize]
            public void Setup()
            {
                _cosineSimilarity = new CosineSimilarity();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void ReadVectorsFromCsv_InvalidFilePath_ThrowsException()
            {
                _cosineSimilarity.ReadVectorsFromCsv(null!);
            }
            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ReadVectorsFromCsv_MalformedData_ThrowsException()
            {
                // Arrange
                string tempFilePath = Path.GetTempFileName();
                File.WriteAllText(tempFilePath, "InvalidDataWithoutVectors");

                try
                {
                    // Act
                    _cosineSimilarity.ReadVectorsFromCsv(tempFilePath);
                }
                finally
                {
                    // Cleanup
                    File.Delete(tempFilePath);
                }
            }
        }
    }
}
