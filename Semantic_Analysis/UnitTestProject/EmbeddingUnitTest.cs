using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnitTestProject
{
    public static class Embedding // Your main class
    {
        public static string ReadJsonFile(string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath))
            {
                throw new FileNotFoundException($"The specified JSON file does not exist: {jsonFilePath}");
            }

            return File.ReadAllText(jsonFilePath);
        }

        public static List<string> AnalyzeJson(string jsonContent)
        {
            var parsedJson = JsonConvert.DeserializeObject(jsonContent);
            var extractedData = new List<string>();

            if (parsedJson == null)
            {
                throw new Exception("The provided JSON content is empty or malformed.");
            }

            void Traverse(object? obj, string prefix = "")
            {
                if (obj is JObject jObject)
                {
                    foreach (var property in jObject.Properties())
                    {
                        Traverse(property.Value, $"{prefix}{property.Name}: ");
                    }
                }
                else if (obj is JArray jArray)
                {
                    int index = 0;
                    foreach (var item in jArray)
                    {
                        Traverse(item, $"{prefix}[{index++}]: ");
                    }
                }
                else if (obj is JValue jValue)
                {
                    extractedData.Add($"{prefix.TrimEnd(' ')}{jValue.Value}");
                }
            }

            Traverse(parsedJson);
            return extractedData;
        }
    }

    [TestClass]
    public class JsonFileReaderUnitTest
    {
        private const string ValidJson = "{\"key\": \"value\"}";
        private string tempFilePath = "test.json";

        [TestInitialize]
        public void Setup()
        {
            File.WriteAllText(tempFilePath, ValidJson);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }

        [TestMethod]
        public void ReadJsonFile_ValidFile_ShouldReturnContent()
        {
            // Act
            string result = Embedding.ReadJsonFile(tempFilePath);

            // Assert
            Assert.AreEqual(ValidJson, result, "The JSON content should match exactly.");
        }

        [TestMethod]
        public void ReadJsonFile_FileDoesNotExist_ShouldThrowFileNotFoundException()
        {
            // Arrange
            string nonExistentFilePath = "nonexistent.json";

            // Act & Assert
            var ex = Assert.ThrowsException<FileNotFoundException>(() => Embedding.ReadJsonFile(nonExistentFilePath));
            Assert.IsTrue(ex.Message.Contains("does not exist"), "Exception message should indicate missing file.");
        }

        [TestMethod]
        public void ReadJsonFile_EmptyFile_ShouldReturnEmptyString()
        {
            // Arrange
            File.WriteAllText(tempFilePath, string.Empty);

            // Act
            string result = Embedding.ReadJsonFile(tempFilePath);

            // Assert
            Assert.AreEqual(string.Empty, result, "An empty JSON file should return an empty string.");
        }
    }

    [TestClass]
    public class JsonAnalyzerUnitTest
    {
        private const string ComplexJson = @"{
            ""person"": {
                ""name"": ""Alice"",
                ""age"": 25,
                ""skills"": [""Java"", ""Python""]
            }
        }";

        [TestMethod]
        public void AnalyzeJson_ValidJson_ShouldExtractCorrectData()
        {
            // Act
            List<string> result = Embedding.AnalyzeJson(ComplexJson);

            // Assert
            CollectionAssert.Contains(result, "person: name: Alice", "Person's name should be extracted correctly.");
            CollectionAssert.Contains(result, "person: age: 25", "Person's age should be extracted correctly.");
            CollectionAssert.Contains(result, "person: skills[0]: Java", "First skill should be extracted correctly.");
            CollectionAssert.Contains(result, "person: skills[1]: Python", "Second skill should be extracted correctly.");
        }

        [TestMethod]
        public void AnalyzeJson_EmptyJson_ShouldReturnEmptyList()
        {
            // Arrange
            string emptyJson = "{}";

            // Act
            List<string> result = Embedding.AnalyzeJson(emptyJson);

            // Assert
            Assert.AreEqual(0, result.Count, "An empty JSON should return an empty list.");
        }

        [TestMethod]
        public void AnalyzeJson_InvalidJson_ShouldThrowJsonReaderException()
        {
            // Arrange
            string invalidJson = "Invalid JSON";

            // Act & Assert
            Assert.ThrowsException<JsonReaderException>(() => Embedding.AnalyzeJson(invalidJson), "Malformed JSON should throw JsonReaderException.");
        }

        [TestMethod]
        public void AnalyzeJson_NullJson_ShouldThrowException()
        {
            // Act & Assert
            var ex = Assert.ThrowsException<Exception>(() => Embedding.AnalyzeJson(null));
            Assert.AreEqual("The provided JSON content is empty or malformed.", ex.Message, "Should return correct error message.");
        }

        [TestMethod]
        public void AnalyzeJson_NestedJson_ShouldExtractData()
        {
            // Arrange
            string nestedJson = @"{
                ""company"": {
                    ""employees"": [
                        { ""name"": ""Alice"", ""role"": ""Engineer"" },
                        { ""name"": ""Bob"", ""role"": ""Manager"" }
                    ]
                }
            }";

            // Act
            List<string> result = Embedding.AnalyzeJson(nestedJson);

            // Assert
            CollectionAssert.Contains(result, "company: employees[0]: name: Alice", "First employee's name should be extracted.");
            CollectionAssert.Contains(result, "company: employees[0]: role: Engineer", "First employee's role should be extracted.");
            CollectionAssert.Contains(result, "company: employees[1]: name: Bob", "Second employee's name should be extracted.");
            CollectionAssert.Contains(result, "company: employees[1]: role: Manager", "Second employee's role should be extracted.");
        }
    }
}
