using Semantic_Analysis;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    // UnitTest class for testing the DataExtraction functionality
    [TestClass]
    public class DataExtractionTest
    {
        // DataExtraction instance to be tested
        private DataExtraction? _dataExtraction;

        // Setup method to initialize DataExtraction before each test
        [TestInitialize]
        public void Setup()
        {
            // Initialize the DataExtraction object
            _dataExtraction = new DataExtraction();
        }

        /// <summary>
        /// Test for the ExtractDataFromFile method that checks if the method
        /// can extract data from a file for any supported file type.
        /// </summary>
        [TestMethod]
        public void ExtractDataFromFile_ShouldReturnNonEmptyData_WhenValidFilePath()
        {
            // Arrange
            string filePath = @"D:\IT\Software Engineering\SE Project\Saquib\semantic-analysis\Semantic_Analysis\Semantic_Analysis\Preprocessing\preprocessing.txt"; // Full file path

            // Check if the test file exists before proceeding
            if (!File.Exists(filePath))
            {
                Assert.Fail($"Test file not found at {filePath}. Please ensure the file exists before running the test.");
            }

            // Act
            if (_dataExtraction == null)
            {
                Assert.Fail("DataExtraction is not initialized.");
            }
            var result = _dataExtraction.ExtractDataFromFile(filePath);

            // Assert: Ensure the result is not empty
            Assert.IsTrue(result.Count > 0, "The file should contain at least one non-empty line of data.");

            // Check if each line is meaningful and not empty
            foreach (var line in result)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(line), "There should be no empty or whitespace-only lines in the file.");
                Assert.IsTrue(line.Length > 2, "Each line should be meaningful (more than 2 characters).");
            }
        }

        /// <summary>
        /// Test for the ExtractDataFromText method, ensuring that the file
        /// is read correctly and no empty lines exist in the extracted data.
        /// </summary>
        [TestMethod]
        public void ExtractDataFromText_ShouldReturnNonEmptyData_WhenValidFilePath()
        {
            // Arrange
            string filePath = @"D:\IT\Software Engineering\SE Project\Saquib\semantic-analysis\Semantic_Analysis\Semantic_Analysis\Preprocessing\preprocessing.txt"; // Example for text file

            // Check if the test file exists before proceeding
            if (!File.Exists(filePath))
            {
                Assert.Fail($"Test file not found at {filePath}. Please ensure the file exists before running the test.");
            }

            // Act
            if (_dataExtraction == null)
            {
                Assert.Fail("DataExtraction is not initialized.");
            }
            var result = _dataExtraction.ExtractDataFromText(filePath);

            // Assert: Ensure the result contains data
            Assert.IsTrue(result.Count > 0, "The file should contain at least one non-empty line of data.");
            foreach (var line in result)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(line), "There should be no empty or whitespace-only lines in the file.");
                Assert.IsTrue(line.Length > 2, "Each line should be meaningful (more than 2 characters).");
            }
        }

        /// <summary>
        /// Test for the ExtractDataFromCsv method, ensuring that the CSV
        /// file is parsed correctly and contains no empty or invalid lines.
        /// </summary>
        [TestMethod]
        public void ExtractDataFromCsv_ShouldReturnNonEmptyData_WhenValidCsvFile()
        {
            // Arrange
            string filePath = @"D:\IT\Software Engineering\SE Project\Saquib\semantic-analysis\Semantic_Analysis\Semantic_Analysis\Preprocessing\data.csv"; // Example for CSV file

            // Check if the test file exists before proceeding
            if (!File.Exists(filePath))
            {
                Assert.Fail($"Test file not found at {filePath}. Please ensure the file exists before running the test.");
            }

            // Act
            if (_dataExtraction == null)
            {
                Assert.Fail("DataExtraction is not initialized.");
            }
            var result = _dataExtraction.ExtractDataFromCsv(filePath);

            // Assert: Ensure the result contains data
            Assert.IsTrue(result.Count > 0, "The file should contain at least one non-empty line of data.");
            foreach (var line in result)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(line), "There should be no empty or whitespace-only lines in the file.");
                Assert.IsTrue(line.Length > 2, "Each line should be meaningful (more than 2 characters).");
            }
        }

        /// <summary>
        /// Test for the ExtractDataFromJson method, ensuring that data is
        /// extracted correctly from the JSON file and contains no empty lines.
        /// </summary>
        [TestMethod]
        public void ExtractDataFromJson_ShouldReturnNonEmptyData_WhenValidJsonFile()
        {
            // Arrange
            string filePath = @"D:\IT\Software Engineering\SE Project\Saquib\semantic-analysis\Semantic_Analysis\Semantic_Analysis\Preprocessing\data.json"; // Example for JSON file

            // Check if the test file exists before proceeding
            if (!File.Exists(filePath))
            {
                Assert.Fail($"Test file not found at {filePath}. Please ensure the file exists before running the test.");
            }

            // Act
            if (_dataExtraction == null)
            {
                Assert.Fail("DataExtraction is not initialized.");
            }
            var result = _dataExtraction.ExtractDataFromJson(filePath);

            // Assert: Ensure the result contains data
            Assert.IsTrue(result.Count > 0, "The file should contain at least one non-empty line of data.");
            foreach (var line in result)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(line), "There should be no empty or whitespace-only lines in the file.");
                Assert.IsTrue(line.Length > 2, "Each line should be meaningful (more than 2 characters).");
            }
        }

        /// <summary>
        /// Test for the ExtractDataFromPdf method, ensuring that data
        /// can be extracted from the PDF file correctly and that there are no
        /// empty or invalid lines in the extracted content.
        /// </summary>
        [TestMethod]
        public void ExtractDataFromPdf_ShouldReturnNonEmptyData_WhenValidPdfFile()
        {
            // Arrange
            string filePath = @"D:\IT\Software Engineering\SE Project\Saquib\semantic-analysis\Semantic_Analysis\Semantic_Analysis\Preprocessing\data.pdf"; // Example for PDF file

            // Check if the test file exists before proceeding
            if (!File.Exists(filePath))
            {
                Assert.Fail($"Test file not found at {filePath}. Please ensure the file exists before running the test.");
            }

            // Act
            if (_dataExtraction == null)
            {
                Assert.Fail("DataExtraction is not initialized.");
            }
            var result = _dataExtraction.ExtractDataFromPdf(filePath);

            // Assert: Ensure the result is not null
            Assert.IsNotNull(result, "The result should not be null.");

            // Check if result is not null before accessing its properties
            if (result != null)
            {
                // Assert: Ensure the result contains data (ensure result is not empty)
                Assert.IsTrue(result.Count > 0, "The PDF file should contain extracted text.");

                // Assert: Ensure there are no empty or whitespace-only lines
                foreach (var line in result)
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(line), "There should be no empty or whitespace-only lines in the extracted PDF text.");
                }
            }
        }

        /// <summary>
        /// Test for the CleanData method, ensuring that special characters
        /// and unnecessary whitespace are removed from the raw data.
        /// </summary>
        [TestMethod]
        public void CleanData_ShouldRemoveSpecialCharactersAndWhitespace()
        {
            // Arrange
            List<string> rawData = new List<string>
            {
                "  Hello World!   ",
                "     This is a test.  ",
                "   Special @# characters!  "
            };

            // Ensure that _dataExtraction is initialized
            Assert.IsNotNull(_dataExtraction, "The DataExtraction object should be initialized.");

            // Act
            var cleanedData = _dataExtraction.CleanData(rawData);

            // Assert: Ensure the cleaned data has no leading/trailing whitespaces or special characters
            foreach (var line in cleanedData)
            {
                Assert.IsFalse(line.Contains("@") || line.Contains("#"), "Special characters should be removed.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(line), "There should be no empty or whitespace-only lines.");
            }
        }
    }
}
