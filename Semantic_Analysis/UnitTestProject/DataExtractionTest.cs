using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Semantic_Analysis; // Assuming this is where DataExtraction is defined
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UnitTestProject
{
    [TestClass]
    public class DataExtractionTest
    {
        private DataExtraction? _dataExtraction; // Nullable to allow for proper initialization
        private string? _testDirectory; // Nullable
        private string? _dataPreprocessingPath; // Nullable
        private string? _extractedDataPath; // Nullable

        // Constructor, ensuring _dataExtraction is initialized
        public DataExtractionTest()
        {
            _dataExtraction = new DataExtraction(); // Ensure _dataExtraction is initialized
        }

        [TestInitialize]
        public void Setup()
        {
            var configuration = LoadConfiguration();

            _dataPreprocessingPath = configuration["FilePaths:DataPreprocessing"];
            _extractedDataPath = configuration["FilePaths:ExtractedData"];

            // Ensure paths are valid before continuing
            Assert.IsFalse(string.IsNullOrEmpty(_dataPreprocessingPath), "DataPreprocessing path should not be empty.");
            Assert.IsFalse(string.IsNullOrEmpty(_extractedDataPath), "ExtractedData path should not be empty.");

            // Ensure paths are resolved relative to the solution root
            var solutionRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\.."));
            _dataPreprocessingPath = Path.Combine(solutionRoot, _dataPreprocessingPath!); // Null-forgiving operator used after checking it's not null
            _extractedDataPath = Path.Combine(solutionRoot, _extractedDataPath!);

            Console.WriteLine($"Resolved DataPreprocessing Path: {_dataPreprocessingPath}");
            Console.WriteLine($"Resolved ExtractedData Path: {_extractedDataPath}");
            Console.WriteLine($"Current working directory: {Directory.GetCurrentDirectory()}");

            // Validate that directories exist
            if (!Directory.Exists(_dataPreprocessingPath))
            {
                Console.WriteLine($"Directory does not exist: {_dataPreprocessingPath}");
                Assert.Fail($"Directory does not exist: {_dataPreprocessingPath}");
            }

            // Create a TestFiles directory for tests if it doesn't exist
            _testDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles");
            if (!Directory.Exists(_testDirectory))
            {
                Directory.CreateDirectory(_testDirectory);
            }

            Console.WriteLine($"Test Directory: {_testDirectory}");
        }

        // Method to load configuration from appsettings.json
        private IConfiguration LoadConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());  // Ensure loading from current directory
            configurationBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            return configurationBuilder.Build();
        }

        // Get any valid file from the DataPreprocessing directory
        private string GetAnyFileFromDirectory(string? directoryPath)
        {
            // Ensure directoryPath is not null or empty before passing it to Directory.GetFiles
            if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
            {
                Console.WriteLine($"Directory does not exist: {directoryPath}");
                return string.Empty;
            }

            // Debugging output to show all files in the directory and the file extensions it recognizes
            Console.WriteLine($"Looking for supported file types: .txt, .csv, .json, .md, .xml, .html, .pdf");

            // Get all files in the directory (including subdirectories if needed)
            var files = Directory.GetFiles(directoryPath)
                                 .Where(file => new[] { ".txt", ".csv", ".json", ".md", ".xml", ".html", ".pdf" }
                                                .Contains(Path.GetExtension(file)?.ToLower()))
                                 .ToList();

            Console.WriteLine($"Files found in directory: {directoryPath}");
            foreach (var file in files)
            {
                Console.WriteLine($"Found file: {file} with extension: {Path.GetExtension(file)}");
            }

            if (files.Count == 0)
            {
                Console.WriteLine($"No valid files found in the directory: {directoryPath}");
            }

            return files.FirstOrDefault() ?? string.Empty;
        }



        // Test method to check that data extraction works with a valid file
        [TestMethod]
        public void ExtractDataFromFile_ShouldReturnNonEmptyData_WhenValidFilePath()
        {
            // Hardcode the file path for debugging
            var filePath = @"D:\IT\Software Engineering\SE Project\Saquib\semantic-analysis\Semantic_Analysis\Semantic_Analysis\RawData\DataSet1.pdf";

            // Log the file path for debugging
            Console.WriteLine($"Hardcoded File Path: {filePath}");

            // Check if the file exists at the hardcoded path
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Assert.Fail($"No valid file found at the specified path. File Path: {filePath}");
            }

            try
            {
                // Act: Extract data from the file
                var result = _dataExtraction?.ExtractDataFromFile(filePath);

                // Assert: Ensure the result is not empty
                Assert.IsTrue(result?.Count > 0, "The file should contain at least one non-empty line of data.");

                // Check if each line is meaningful and not empty
                foreach (var line in result)
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(line), "There should be no empty or whitespace-only lines in the file.");
                    Assert.IsTrue(line.Length > 2, "Each line should be meaningful (more than 2 characters).");
                }
            }
            catch (Exception ex)
            {
                Assert.Fail($"Extraction failed for file {filePath}: {ex.Message}");
            }
        }



        // Test method to clean data and remove special characters and whitespace
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

            try
            {
                // Act: Clean the data
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
            catch (Exception ex)
            {
                Assert.Fail($"Data cleaning failed: {ex.Message}");
            }
        }

        // Cleanup method to remove test directory after tests
        [TestCleanup]
        public void Cleanup()
        {
            // Clean up test directory after tests (optional)
            if (Directory.Exists(_testDirectory!)) // Use null-forgiving operator here after validation
            {
                Directory.Delete(_testDirectory!, true);
            }
        }
    }
}
