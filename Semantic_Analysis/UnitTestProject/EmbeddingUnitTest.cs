using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Semantic_Analysis.Interfaces;
using OpenAI.Embeddings;

namespace UnitTestProject
{
    [TestClass]
    public class EmbeddingProcessorTests
    {
        private string tempJsonPath = "test.json";
        private string tempCsvPath = "test_output.csv";
        private EmbeddingProcessor processor;

        [TestInitialize]
        public void Setup()
        {
            processor = new EmbeddingProcessor();
            File.WriteAllText(tempJsonPath, "{\"key\": \"value\"}");
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(tempJsonPath)) File.Delete(tempJsonPath);
            if (File.Exists(tempCsvPath)) File.Delete(tempCsvPath);
        }

        [TestMethod]
        public async Task ReadJsonFile_ValidFile_ShouldReturnContent()
        {
            string result = await processor.ReadJsonFileAsync(tempJsonPath);
            Assert.AreEqual("{\"key\": \"value\"}", result);
        }

        [TestMethod]
        public async Task ReadJsonFile_FileDoesNotExist_ShouldThrowException()
        {
            string fakePath = "missing.json";
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(async () => await processor.ReadJsonFileAsync(fakePath));
        }

        [TestMethod]
        public void AnalyzeJson_ValidJson_ShouldExtractKeyValue()
        {
            // Arrange
            var processor = new EmbeddingProcessor();
            string json = "{\"name\": \"John\"}";

            // Act
            List<string> result = processor.AnalyzeJson(json);

            // Assert
            CollectionAssert.Contains(result, "name:John"); // Adjusted based on function output
        }

        [TestMethod]
        public void AnalyzeJson_InvalidJson_ShouldThrowException()
        {
            string invalidJson = "Invalid JSON";
            Assert.ThrowsException<JsonReaderException>(() => processor.AnalyzeJson(invalidJson));
        }

        [TestMethod]
        public async Task GenerateAndSaveEmbeddingsAsync_ShouldCreateCSV()
        {
            var descriptions = new List<string> { "Test description" };
            await processor.GenerateAndSaveEmbeddingsAsync("fake-api-key", descriptions, tempCsvPath, 1);
            Assert.IsTrue(File.Exists(tempCsvPath));
        }

        [TestMethod]
        public async Task ProcessJsonFileAsync_ShouldProcessAndSaveEmbeddings()
        {
            string apiKey = "fake-api-key";
            await processor.ProcessJsonFileAsync(tempJsonPath, tempCsvPath, apiKey, 1);
            Assert.IsTrue(File.Exists(tempCsvPath));
        }
    }
}