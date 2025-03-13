using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Semantic_Analysis; // Assuming this is where DataExtraction is defined

namespace UnitTestProject
{
    [TestClass]
    public class DataExtractionTest
    {
        private DataExtraction _dataExtraction;
        private string? _dataPreprocessingPath;
        private string? _extractedDataPath;
        private List<string>? _supportedFileExtensions;

        public DataExtractionTest()
        {
            _dataExtraction = new DataExtraction();
        }

        [TestInitialize]
        public void Setup()
        {
            // Load configuration
            var configuration = LoadConfiguration();

            // Get the file paths from configuration
            _dataPreprocessingPath = configuration["FilePaths:DataPreprocessing"];
            _extractedDataPath = configuration["FilePaths:ExtractedData"];
            _supportedFileExtensions = configuration["FilePaths:SupportedFileExtensions"]?
                                      .Split(',')
                                      .Select(ext => ext.Trim().ToLower())
                                      .ToList() ?? new List<string>();

            // Ensure paths and extensions are valid before continuing
            Assert.IsFalse(string.IsNullOrEmpty(_dataPreprocessingPath), "DataPreprocessing path should not be empty.");
            Assert.IsFalse(string.IsNullOrEmpty(_extractedDataPath), "ExtractedData path should not be empty.");
            Assert.IsTrue(_supportedFileExtensions?.Any() ?? false, "There should be at least one supported file extension.");

            // Validate directories
            var solutionRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\.."));

            // Ensure the _dataPreprocessingPath points to RawData directory, which contains your test files
            _dataPreprocessingPath = Path.Combine(solutionRoot, "Semantic_Analysis", "Semantic_Analysis", "RawData");
            _extractedDataPath = Path.Combine(solutionRoot, _extractedDataPath ?? string.Empty);

            // Ensure the RawData directory exists
            Assert.IsTrue(Directory.Exists(_dataPreprocessingPath), $"Directory does not exist: {_dataPreprocessingPath}");

            Assert.IsTrue(Directory.Exists(_extractedDataPath), $"Directory does not exist: {_extractedDataPath}");
        }

        // Get a valid file from the directory
        private string GetAnyFileFromDirectory(string? directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
            {
                Console.WriteLine($"Directory does not exist or is invalid: {directoryPath}");
                return string.Empty;
            }

            Console.WriteLine($"Looking for supported file types: .txt, .csv, .json, .md, .xml, .html, .pdf, .docx");

            var files = Directory.GetFiles(directoryPath)
                                 .Where(file =>
                                 {
                                     var extension = Path.GetExtension(file)?.ToLower();

                                     Console.WriteLine($"Checking file: {file} with extension {extension}");

                                     // Skip temporary files
                                     if (Path.GetFileName(file).StartsWith("~$"))
                                     {
                                         return false;
                                     }

                                     // Check if the file extension is supported
                                     return _supportedFileExtensions?.Contains(extension, StringComparer.OrdinalIgnoreCase) ?? false;
                                 })
                                 .ToList();

            Console.WriteLine($"Files found: {string.Join(", ", files ?? new List<string>())}");

            if (files == null || !files.Any())
            {
                Console.WriteLine($"No valid files found in the directory: {directoryPath}");
                return string.Empty;
            }

            return files.FirstOrDefault() ?? string.Empty;
        }


        // Manual parsing of appsettings.json
        private Dictionary<string, string> LoadConfiguration()
        {
            var configuration = new Dictionary<string, string>();

            // Read the appsettings.json file
            var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

            if (!File.Exists(appSettingsPath))
            {
                throw new FileNotFoundException("appsettings.json not found in the expected location.");
            }

            var jsonString = File.ReadAllText(appSettingsPath);

            try
            {
                // Parse the JSON content
                using (JsonDocument doc = JsonDocument.Parse(jsonString))
                {
                    // Extract the required data from the JSON file
                    var filePaths = doc.RootElement.GetProperty("FilePaths");

                    // Ensure that the properties exist and handle possible nulls
                    configuration["FilePaths:DataPreprocessing"] = filePaths.TryGetProperty("DataPreprocessing", out var dataPreprocessingProp)
                        ? dataPreprocessingProp.GetString() ?? string.Empty
                        : string.Empty;

                    configuration["FilePaths:ExtractedData"] = filePaths.TryGetProperty("ExtractedData", out var extractedDataProp)
                        ? extractedDataProp.GetString() ?? string.Empty
                        : string.Empty;

                    // Handle SupportedFileExtensions as an array
                    var extensionsArray = filePaths.GetProperty("SupportedFileExtensions").EnumerateArray();
                    var supportedExtensions = extensionsArray.Select(ext => ext.GetString()?.Trim().ToLower()).ToList();
                    configuration["FilePaths:SupportedFileExtensions"] = string.Join(",", supportedExtensions);
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error parsing appsettings.json: {ex.Message}");
                throw; // Rethrow to fail the test if configuration is invalid
            }

            return configuration;
        }



        [TestMethod]
        public void ExtractDataFromFile_ShouldReturnNonEmptyData_WhenValidFilePath()
        {
            // Arrange: Get any file from the DataPreprocessing directory
            var filePath = GetAnyFileFromDirectory(_dataPreprocessingPath);

            // Log the file path for debugging
            Console.WriteLine($"File Path: {filePath}");

            // Check if the test file exists before proceeding
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Assert.Fail($"No valid file found in the DataPreprocessing directory.");
            }

            try
            {
                // Ensure _dataExtraction is not null
                Assert.IsNotNull(_dataExtraction, "DataExtraction is not initialized.");

                // Act: Extract data from the file
                Console.WriteLine($"Starting data extraction for file: {filePath}");
                var result = _dataExtraction.ExtractDataFromFile(filePath);

                // Log the result for debugging
                Console.WriteLine($"Data extracted: {string.Join(", ", result)}");

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
                Assert.Fail($"Data extraction failed for file {filePath}: {ex.Message}");
            }
        }
    }
}

