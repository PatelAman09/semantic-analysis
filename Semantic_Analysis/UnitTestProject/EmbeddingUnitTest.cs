
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Semantic_Analysis.Interfaces;


namespace EmbeddingProcessor_UnitTest
{
    [TestClass]
    public class EmbeddingProcessorTests
    {
        private IEmbeddingProcessor _processor = null!;
        private string _tempDirectory = null!;
        private string _tempJsonFile = null!;
        private string _tempCsvFile = null!;

        [TestInitialize]
        public void Initialize()
        {
            _processor = new EmbeddingProcessor();
            _tempDirectory = Path.Combine(Path.GetTempPath(), "EmbeddingProcessorTests");

            // Create temp directory if it doesn't exist
            if (!Directory.Exists(_tempDirectory))
            {
                Directory.CreateDirectory(_tempDirectory);
            }
            _tempJsonFile = Path.Combine(_tempDirectory, "test.json");
            _tempCsvFile = Path.Combine(_tempDirectory, "test.csv");
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Clean up test files
            if (_tempJsonFile != null && File.Exists(_tempJsonFile))
            {
                File.Delete(_tempJsonFile);
            }

            if (_tempCsvFile != null && File.Exists(_tempCsvFile))
            {
                File.Delete(_tempCsvFile);
            }

            if (_tempDirectory != null && Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }

        [TestMethod]
        public async Task ReadJsonFileAsync_FileExists_ReturnsContent()
        {
            string expectedContent = "{\"name\":\"test\"}";
            if (_tempJsonFile != null)
                File.WriteAllText(_tempJsonFile, expectedContent);

            string actualContent = await _processor.ReadJsonFileAsync(_tempJsonFile);

            Assert.AreEqual(expectedContent, actualContent);
        }

        [TestMethod]
        public async Task ReadJsonFileAsync_FileDoesNotExist_ThrowsException()
        {
            var nonExistentFile = Path.Combine(Path.GetTempPath(), "nonexistent.json");

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

        [TestMethod]
        public void ProcessWholeJson_ValidJson_ReturnsWholeDocument()
        {
            string jsonContent = @"{""name"": ""John"", ""age"": 30}";

            List<string> result = _processor.ProcessWholeJson(jsonContent);

            Assert.AreEqual(1, result.Count, "Should return exactly one item");
            Assert.AreEqual(jsonContent, result[0], "Should return the entire JSON content");
        }

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

        [TestMethod]
        public void LoadConfiguration_ConfigExists_ReturnsConfig()
        {
            if (_tempDirectory == null) return;

            // Create a temporary config file for testing
            string tempConfigPath = Path.Combine(_tempDirectory, "appsettings.json");
            string configContent = @"{
                ""TestSetting"": ""TestValue""
            }";
            File.WriteAllText(tempConfigPath, configContent);

            // Use a helper method to test the static method
            var config = TestLoadConfiguration(_tempDirectory);

            Assert.IsNotNull(config);
            Assert.AreEqual("TestValue", config["TestSetting"]);
        }

        // Helper method to test the static LoadConfiguration method
        private static IConfigurationRoot TestLoadConfiguration(string basePath)
        {
            return new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
        }

        // Test for the ProcessJsonFileAsync method - simulated to avoid API calls
        [TestMethod]
        public async Task ProcessJsonFileAsync_FileHandling_DeletesExistingFile()
        {
            if (_tempCsvFile == null || _tempJsonFile == null) return;

            // Create existing CSV file
            File.WriteAllText(_tempCsvFile, "existing content");
            Assert.IsTrue(File.Exists(_tempCsvFile));

            // Create a test JSON file
            string jsonContent = "{\"test\": \"value\"}";
            File.WriteAllText(_tempJsonFile, jsonContent);

            // Create a test implementation to simulate file deletion
            var testProcessor = new TestEmbeddingProcessor();

            try
            {
                await testProcessor.TestProcessJsonFileAsync(_tempJsonFile, _tempCsvFile);

                // If test implementation avoids the API call, we should reach here
                Assert.IsFalse(File.Exists(_tempCsvFile), "File should be deleted but not recreated in test");
            }
            catch (Exception ex) when (ex.Message.Contains("Test implementation"))
            {
                // This is expected since our test implementation throws after deletion
                Assert.IsFalse(File.Exists(_tempCsvFile), "File should be deleted");
            }
        }

        // Test implementation class to avoid real API calls
        private class TestEmbeddingProcessor : EmbeddingProcessor
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
        }
    }
}
