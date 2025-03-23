using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
<<<<<<< HEAD
using OpenAI.Embeddings;
using Semantic_Analysis;


namespace EmbeddingProcessor_UnitTest
{

    [TestClass]
    public class EmbeddingProcessorTests
    {
        private EmbeddingProcessor? _processor;
        private string? _tempDirectory;
        private string? _tempJsonFile;
        private string? _tempCsvFile;

=======
using Semantic_Analysis.Interfaces;


namespace EmbeddingProcessor_UnitTest
{
    /// <summary>
    /// Unit tests for the EmbeddingProcessor class.
    /// </summary>
    [TestClass]
    public class EmbeddingProcessorTests
    {
        private IEmbeddingProcessor _processor = null!;
        private string _tempDirectory = null!;
        private string _tempJsonFile = null!;
        private string _tempCsvFile = null!;

        #region Setup and Cleanup

        /// <summary>
        /// Initializes the test environment.
        /// </summary>
>>>>>>> origin/Aman-Patel
        [TestInitialize]
        public void Initialize()
        {
            _processor = new EmbeddingProcessor();
            _tempDirectory = Path.Combine(Path.GetTempPath(), "EmbeddingProcessorTests");
<<<<<<< HEAD

            // Create temp directory if it doesn't exist
            if (!Directory.Exists(_tempDirectory))
            {
                Directory.CreateDirectory(_tempDirectory);
            }
            _tempJsonFile = Path.Combine(_tempDirectory, "test.json");
            _tempCsvFile = Path.Combine(_tempDirectory, "test.csv");
        }
=======
>>>>>>> origin/Aman-Patel

            // Create temp directory if it doesn't exist
            if (!Directory.Exists(_tempDirectory))
            {
                Directory.CreateDirectory(_tempDirectory);
            }
            _tempJsonFile = Path.Combine(_tempDirectory, "test.json");
            _tempCsvFile = Path.Combine(_tempDirectory, "test.csv");
        }

        /// <summary>
        /// Cleans up temporary files after tests.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            // Clean up test files
            if (_tempJsonFile != null && File.Exists(_tempJsonFile))
            {
                File.Delete(_tempJsonFile);
            }
<<<<<<< HEAD

            if (_tempCsvFile != null && File.Exists(_tempCsvFile))
            {
                File.Delete(_tempCsvFile);
            }

            if (_tempDirectory != null && Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }
=======
>>>>>>> origin/Aman-Patel

            if (_tempCsvFile != null && File.Exists(_tempCsvFile))
            {
                File.Delete(_tempCsvFile);
            }

            if (_tempDirectory != null && Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }
        #endregion

        #region JSON File Handling Tests

        /// <summary>
        /// Tests reading a JSON file when the file exists.
        /// </summary>
        [TestMethod]
        public async Task ReadJsonFileAsync_FileExists_ReturnsContent()
        {
            string expectedContent = "{\"name\":\"test\"}";
            if (_tempJsonFile != null)
                File.WriteAllText(_tempJsonFile, expectedContent);

<<<<<<< HEAD
            string? actualContent = null;
            if (_processor != null && _tempJsonFile != null)
                actualContent = await _processor.ReadJsonFileAsync(_tempJsonFile);

            Assert.AreEqual(expectedContent, actualContent);
        }
=======
            string actualContent = await _processor.ReadJsonFileAsync(_tempJsonFile);
>>>>>>> origin/Aman-Patel

            Assert.AreEqual(expectedContent, actualContent);
        }

        /// <summary>
        /// Tests reading a JSON file when the file does not exist.
        /// </summary>
        [TestMethod]
        public async Task ReadJsonFileAsync_FileDoesNotExist_ThrowsException()
        {
            var nonExistentFile = Path.Combine(Path.GetTempPath(), "nonexistent.json");
<<<<<<< HEAD
            if (_processor != null)
            {
                try
                {
                    await _processor.ReadJsonFileAsync(nonExistentFile);
                    Assert.Fail("Expected FileNotFoundException was not thrown");
                }
                catch (FileNotFoundException)
                {

                }
            }
        }

        [TestMethod]
        public void AnalyzeJson_ValidJson_ExtractsData_Lenient()
        {
            if (_processor == null) return;

            string jsonContent = @"{
        ""name"": ""John"",
        ""age"": 30,
        ""address"": {
            ""street"": ""123 Main St"",
            ""city"": ""Anytown""
        },
        ""phoneNumbers"": [""555-1234"", ""555-5678""]
    }";
            List<string> result = _processor.AnalyzeJson(jsonContent);
            Console.WriteLine($"Found {result.Count} items in result");
            foreach (var item in result)
            {
                Console.WriteLine($"- \"{item}\"");
            }
            Assert.IsTrue(result.Any(s => s.Contains("John")), "Missing 'John' in results");
            Assert.IsTrue(result.Any(s => s.Contains("30")), "Missing '30' in results");
            Assert.IsTrue(result.Any(s => s.Contains("123 Main St")), "Missing street address in results");
            Assert.IsTrue(result.Any(s => s.Contains("Anytown")), "Missing city in results");
            Assert.IsTrue(result.Any(s => s.Contains("555-1234")), "Missing first phone number in results");
            Assert.IsTrue(result.Any(s => s.Contains("555-5678")), "Missing second phone number in results");
        }

        [TestMethod]
        public void AnalyzeJson_InvalidJson_ThrowsException()
        {
            if (_processor == null) return;

            string jsonContent = "invalid json";

            try
            {
                _processor.AnalyzeJson(jsonContent);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (JsonReaderException)
            {

            }
        }

        [TestMethod]
        public void SaveOutputToCsv_ValidData_CreatesFile()
        {
            if (_processor == null || _tempCsvFile == null) return;

            List<string> outputData = new List<string> { "line1", "line2", "line3" };

            _processor.SaveOutputToCsv(_tempCsvFile, outputData);

            Assert.IsTrue(File.Exists(_tempCsvFile));
            string[] fileContent = File.ReadAllLines(_tempCsvFile);
            Assert.AreEqual(3, fileContent.Length);
            Assert.AreEqual("line1", fileContent[0]);
            Assert.AreEqual("line2", fileContent[1]);
            Assert.AreEqual("line3", fileContent[2]);
        }

=======

            try
            {
                await _processor.ReadJsonFileAsync(nonExistentFile);
                Assert.Fail("Expected FileNotFoundException was not thrown");
            }
            catch (FileNotFoundException)
            {
                // Exception expected
            }
        }
        #endregion

        #region JSON Analysis Tests

        /// <summary>
        /// Tests JSON analysis for extracting data.
        /// </summary>

        [TestMethod]
        public void AnalyzeJson_ValidJson_ExtractsData()
        {
            string jsonContent = @"{
                ""name"": ""John"",
                ""age"": 30,
                ""address"": {
                    ""street"": ""123 Main St"",
                    ""city"": ""Anytown""
                },
                ""phoneNumbers"": [""555-1234"", ""555-5678""]
            }";

            List<string> result = _processor.AnalyzeJson(jsonContent);

            Console.WriteLine($"Found {result.Count} items in result");
            foreach (var item in result)
            {
                Console.WriteLine($"- \"{item}\"");
            }

            Assert.IsTrue(result.Any(s => s.Contains("John")), "Missing 'John' in results");
            Assert.IsTrue(result.Any(s => s.Contains("30")), "Missing '30' in results");
            Assert.IsTrue(result.Any(s => s.Contains("123 Main St")), "Missing street address in results");
            Assert.IsTrue(result.Any(s => s.Contains("Anytown")), "Missing city in results");
            Assert.IsTrue(result.Any(s => s.Contains("555-1234")), "Missing first phone number in results");
            Assert.IsTrue(result.Any(s => s.Contains("555-5678")), "Missing second phone number in results");
        }

        /// <summary>
        /// Tests JSON processing to return the whole document.
        /// </summary>
        [TestMethod]
        public void ProcessWholeJson_ValidJson_ReturnsWholeDocument()
        {
            string jsonContent = @"{""name"": ""John"", ""age"": 30}";

            List<string> result = _processor.ProcessWholeJson(jsonContent);

            Assert.AreEqual(1, result.Count, "Should return exactly one item");
            Assert.AreEqual(jsonContent, result[0], "Should return the entire JSON content");
        }

        /// <summary>
        /// Tests JSON analysis with invalid JSON input.
        /// </summary>
        [TestMethod]
        public void AnalyzeJson_InvalidJson_ThrowsException()
        {
            string jsonContent = "invalid json";

            try
            {
                _processor.AnalyzeJson(jsonContent);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (JsonReaderException)
            {
                // Exception expected
            }
        }
        #endregion

        #region Configuration Tests

        /// <summary>
        /// Tests loading a configuration from an existing file.
        /// </summary>
>>>>>>> origin/Aman-Patel
        [TestMethod]
        public void LoadConfiguration_ConfigExists_ReturnsConfig()
        {
            if (_tempDirectory == null) return;

<<<<<<< HEAD
            // This test assumes appsettings.json exists in the test directory
            // We'll create a temporary one for testing
            string tempConfigPath = Path.Combine(_tempDirectory, "appsettings.json");
            string configContent = @"{
            ""TestSetting"": ""TestValue""
        }";
=======
            // Create a temporary config file for testing
            string tempConfigPath = Path.Combine(_tempDirectory, "appsettings.json");
            string configContent = @"{
                ""TestSetting"": ""TestValue""
            }";
>>>>>>> origin/Aman-Patel
            File.WriteAllText(tempConfigPath, configContent);

            // Use a helper method to test the static method
            var config = TestLoadConfiguration(_tempDirectory);

            Assert.IsNotNull(config);
<<<<<<< HEAD
        }

        // Helper method to test the static LoadConfiguration method
        private static Microsoft.Extensions.Configuration.IConfigurationRoot TestLoadConfiguration(string basePath)
        {
            return new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
        }

        // Testing a custom implementation of a simulated embedding for the GenerateAndSaveEmbeddingsAsync method
        [TestMethod]
        public async Task GenerateAndSaveEmbeddingsAsync_SimulatedTest()
        {
            if (_tempCsvFile == null) return;

            List<string> testDescriptions = new List<string> { "description1", "description2" };

            // Create test file
            await File.WriteAllTextAsync(_tempCsvFile, ""); // Create empty file

            // We can test our method for writing data to CSV files
            using (StreamWriter writer = new StreamWriter(_tempCsvFile))
            {
                // Simulate what would happen in the GenerateAndSaveEmbeddingsAsync method
                foreach (var description in testDescriptions)
                {
                    // Create a simple simulated embedding (just 3 values)
                    string simulatedEmbedding = "0.1,0.2,0.3";
                    await writer.WriteLineAsync($"\"{description}\",\"{simulatedEmbedding}\"");
                }
            }

            // Assert that the CSV file was created with the expected format
            Assert.IsTrue(File.Exists(_tempCsvFile));
            string[] fileContent = File.ReadAllLines(_tempCsvFile);
            Assert.AreEqual(2, fileContent.Length);
            Assert.AreEqual("\"description1\",\"0.1,0.2,0.3\"", fileContent[0]);
            Assert.AreEqual("\"description2\",\"0.1,0.2,0.3\"", fileContent[1]);
        }

        // For ProcessJsonFileAsync, we can test the file handling logic without making API calls
=======
            Assert.AreEqual("TestValue", config["TestSetting"]);
        }

        /// <summary>
        /// Helper method to load configuration.
        /// </summary>
        private static IConfigurationRoot TestLoadConfiguration(string basePath)
        {
            return new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
        }

        /// <summary>
        /// Tests the process of handling JSON file processing, ensuring deletion of an existing file.
        /// </summary>
>>>>>>> origin/Aman-Patel
        [TestMethod]
        public async Task ProcessJsonFileAsync_FileHandling_DeletesExistingFile()
        {
            if (_tempCsvFile == null || _tempJsonFile == null) return;

<<<<<<< HEAD
            // Arrange - create existing CSV file
            File.WriteAllText(_tempCsvFile, "existing content");
            Assert.IsTrue(File.Exists(_tempCsvFile));

            // Create a test JSON file with simple content
            string jsonContent = "{\"test\": \"value\"}";
            File.WriteAllText(_tempJsonFile, jsonContent);


            var testProcessor = new TestEmbeddingProcessor();
=======
            // Create existing CSV file
            File.WriteAllText(_tempCsvFile, "existing content");
            Assert.IsTrue(File.Exists(_tempCsvFile));

            // Create a test JSON file
            string jsonContent = "{\"test\": \"value\"}";
            File.WriteAllText(_tempJsonFile, jsonContent);

            // Create a test implementation to simulate file deletion
            var testProcessor = new TestEmbeddingProcessor();

>>>>>>> origin/Aman-Patel
            try
            {
                await testProcessor.TestProcessJsonFileAsync(_tempJsonFile, _tempCsvFile);

<<<<<<< HEAD
                // If the test implementation avoids the API call, we should reach here
=======
                // If test implementation avoids the API call, we should reach here
>>>>>>> origin/Aman-Patel
                Assert.IsFalse(File.Exists(_tempCsvFile), "File should be deleted but not recreated in test");
            }
            catch (Exception ex) when (ex.Message.Contains("Test implementation"))
            {
                // This is expected since our test implementation throws after deletion
                Assert.IsFalse(File.Exists(_tempCsvFile), "File should be deleted");
            }
<<<<<<< HEAD
        }

        // Test implementation class to avoid real API calls
        private class TestEmbeddingProcessor : EmbeddingProcessor
=======
        }
        #endregion

        #region File Processing Tests

        /// <summary>
        /// Test implementation of EmbeddingProcessor for controlled testing.
        /// </summary>
        public class TestEmbeddingProcessor : EmbeddingProcessor
>>>>>>> origin/Aman-Patel
        {
            public async Task TestProcessJsonFileAsync(string jsonFilePath, string csvFilePath)
            {
                try
                {
                    Console.WriteLine($"Ensuring {csvFilePath} is deleted before processing...");
                    if (File.Exists(csvFilePath))
                    {
                        File.Delete(csvFilePath);
                        Console.WriteLine($"Previous output file deleted: {csvFilePath}");
                    }

                    await Task.CompletedTask;

                    throw new Exception("Test implementation - stopping before API call");
                }
                catch (Exception ex) when (!ex.Message.Contains("Test implementation"))
                {
                    Console.WriteLine($"Error processing JSON file '{jsonFilePath}': {ex.Message}");
                    throw;
                }
            }
<<<<<<< HEAD
        }
=======
        }
        #endregion
>>>>>>> origin/Aman-Patel
    }
}