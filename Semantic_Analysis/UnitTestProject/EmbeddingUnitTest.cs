using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Semantic_Analysis;
using Semantic_Analysis.Interfaces;
using OpenAI.Embeddings;

namespace UnitTestProject
{
    [TestClass]
    public class EmbeddingProcessorTests
    {
        private readonly string tempJsonPath = "test.json";
        private readonly string tempCsvPath = "test_output.csv";
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
            // Act
            string result = await processor.ReadJsonFileAsync(tempJsonPath);

            // Assert
            Assert.AreEqual("{\"key\": \"value\"}", result);
        }

        [TestMethod]
        public async Task ReadJsonFile_FileDoesNotExist_ShouldThrowException()
        {
            string fakePath = "missing.json";
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(async () =>
                await processor.ReadJsonFileAsync(fakePath));
        }

        [TestMethod]
        public void AnalyzeJson_ValidJson_ShouldExtractKeyValue()
        {
            // Arrange
            string json = "{\"name\": \"John\"}";

            // Act
            List<string> result = processor.AnalyzeJson(json);

            // Assert
            CollectionAssert.Contains(result, "name: John"); // Adjusted for correct function output
        }

        [TestMethod]
        public void AnalyzeJson_InvalidJson_ShouldThrowException()
        {
            // Arrange
            string invalidJson = "Invalid JSON";

            // Assert
            Assert.ThrowsException<JsonReaderException>(() => processor.AnalyzeJson(invalidJson));
        }

        [TestMethod]
        public async Task GenerateAndSaveEmbeddingsAsync_ShouldCreateCSV()
        {
            // Arrange
            var descriptions = new List<string> { "Test description" };

            // Act
            await processor.GenerateAndSaveEmbeddingsAsync("fake-api-key", descriptions, tempCsvPath, 1);

            // Assert
            Assert.IsTrue(File.Exists(tempCsvPath));
        }
    }
}
