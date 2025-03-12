using Semantic_Analysis;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UnitTestProject
{
    [TestClass]
    public class DataExtractionTest
    {
        private DataExtraction? _dataExtraction;

        // Mark _testDirectory as nullable to fix CS8618 warning
        private string? _testDirectory;
        private string? _dataPreprocessingPath;
        private string? _extractedDataPath;

        [TestInitialize]
        public void Setup()
        {
            // Initialize DataExtraction object
            _dataExtraction = new DataExtraction();
            Assert.IsNotNull(_dataExtraction, "Failed to initialize DataExtraction.");

            // Load configuration settings from appsettings.json
            var configuration = LoadConfiguration();
            
            // Retrieve paths from configuration
            _dataPreprocessingPath = configuration["FilePaths:DataPreprocessing"];
            _extractedDataPath = configuration["FilePaths:ExtractedData"];

            // Set up a temporary directory for test files
            _testDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles");

            // Create directory if it doesn't exist
            if (!Directory.Exists(_testDirectory))
            {
                Directory.CreateDirectory(_testDirectory);
            }

            Console.WriteLine($"DataPreprocessing Path: {_dataPreprocessingPath}");
            Console.WriteLine($"ExtractedData Path: {_extractedDataPath}");
        }

        private IConfiguration LoadConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
            configurationBuilder.AddJsonFile("appsettings.json");
            return configurationBuilder.Build();
        }

        [TestMethod]
        public void ExtractDataFromFile_ShouldReturnNonEmptyData_WhenValidFilePath()
        {
            // Arrange: Get any file from the DataPreprocessing directory
            var filePath = GetAnyFileFromDirectory(_dataPreprocessingPath!);

            // Log the current working directory for debugging
            Console.WriteLine($"Current working directory: {Directory.GetCurrentDirectory()}");
            Console.WriteLine($"File Path: {filePath}");

            // Check if the test file exists before proceeding
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Assert.Fail("No valid file found in the DataPreprocessing directory.");
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

        private string GetAnyFileFromDirectory(string directoryPath)
        {
            // Search for any file in the specified directory with the supported extensions
            var supportedExtensions = new[] { ".txt", ".csv", ".json", ".md", ".xml", ".html", ".pdf" };

            var files = Directory.GetFiles(directoryPath)
                .Where(file => supportedExtensions.Contains(Path.GetExtension(file)?.ToLower()))
                .ToList();

            // Return the first file found or null if no valid files are found
            return files.FirstOrDefault();
        }

        [TestMethod]
        public void ExtractDataFromCsv_ShouldReturnNonEmptyData_WhenValidCsvFile()
        {
            // Arrange: Create a temporary file with dynamic name
            string fileName = "testCsvFile_" + Guid.NewGuid().ToString() + ".csv";
            string filePath = Path.Combine(_testDirectory!, fileName); // Use null-forgiving operator

            // Create a CSV file with sample data
            File.WriteAllLines(filePath, new List<string>
            {
                "Column1,Column2,Column3",
                "Value1,Value2,Value3",
                "Value4,Value5,Value6"
            });

            // Act
            var result = _dataExtraction?.ExtractDataFromCsv(filePath);

            // Assert: Ensure the result is not empty
            Assert.IsNotNull(result, "The result should not be null.");
            Assert.IsTrue(result.Count > 0, "The CSV file should contain at least one non-empty line of data.");
            foreach (var line in result)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(line), "There should be no empty or whitespace-only lines.");
                Assert.IsTrue(line.Length > 2, "Each line should be meaningful (more than 2 characters).");
            }

            // Cleanup: Delete the test file
            File.Delete(filePath);
        }

        [TestMethod]
        public void ExtractDataFromJson_ShouldReturnNonEmptyData_WhenValidJsonFile()
        {
            // Arrange: Use the path from appsettings.json
            var filePath = GetAnyFileFromDirectory(_dataPreprocessingPath!);

            // Debugging: Print out the file path and current working directory
            Console.WriteLine($"Current working directory: {Directory.GetCurrentDirectory()}");
            Console.WriteLine($"File Path: {filePath}");

            // Check if the test file exists before proceeding
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Assert.Fail("No valid JSON file found in the DataPreprocessing directory.");
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

        [TestMethod]
        public void ExtractDataFromPdf_ShouldReturnNonEmptyData_WhenValidPdfFile()
        {
            // Arrange: Use the path from appsettings.json
            var filePath = GetAnyFileFromDirectory(_dataPreprocessingPath!);

            // Debugging: Print out the file path and current working directory
            Console.WriteLine($"Current working directory: {Directory.GetCurrentDirectory()}");
            Console.WriteLine($"File Path: {filePath}");

            // Check if the test file exists before proceeding
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Assert.Fail("No valid PDF file found in the DataPreprocessing directory.");
            }

            // Act
            if (_dataExtraction == null)
            {
                Assert.Fail("DataExtraction is not initialized.");
            }
            var result = _dataExtraction.ExtractDataFromPdf(filePath);

            // Assert: Ensure the result is not null
            Assert.IsNotNull(result, "The result should not be null.");

            // Assert: Ensure the result contains data
            if (result != null)
            {
                Assert.IsTrue(result.Count > 0, "The PDF file should contain extracted text.");

                // Assert: Ensure there are no empty or whitespace-only lines
                foreach (var line in result)
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(line), "There should be no empty or whitespace-only lines in the extracted PDF text.");
                }
            }
        }

        [TestMethod]
        public void CleanData_ShouldRemoveSpecialCharactersAndWhitespace()
        {
            List<string> rawData = new List<string>
            {
                "  Hello World!   ",
                "     This is a test.  ",
                "   Special @# characters!  "
            };

            // Ensure that _dataExtraction is initialized
            Assert.IsNotNull(_dataExtraction, "The DataExtraction object should be initialized.");

            // Act
            var cleanedData = _dataExtraction?.CleanData(rawData);

            // Assert: Ensure the cleaned data has no leading/trailing whitespaces or special characters
            Assert.IsNotNull(cleanedData, "Cleaned data should not be null.");
            foreach (var line in cleanedData)
            {
                // Assert that no special characters like @ or # are present
                Assert.IsFalse(line.Contains("@") || line.Contains("#"), "Special characters should be removed.");

                // Assert that there are no leading or trailing whitespaces and the line is not empty
                Assert.IsFalse(string.IsNullOrWhiteSpace(line), "There should be no empty or whitespace-only lines.");
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Clean up test directory after tests (optional)
            if (Directory.Exists(_testDirectory!))
            {
                Directory.Delete(_testDirectory!, true);
            }
        }
    }
}
