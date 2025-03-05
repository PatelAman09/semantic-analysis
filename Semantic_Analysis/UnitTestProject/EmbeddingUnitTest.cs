//using System;
//using System.Collections.Generic;
//using System.IO;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;

//namespace UnitTestProject
//{
//    public static class Embedding // Your main class
//    {
//        public static string ReadJsonFile(string jsonFilePath)
//        {
//            if (!File.Exists(jsonFilePath))
//            {
//                throw new FileNotFoundException($"The specified JSON file does not exist: {jsonFilePath}");
//            }

//            return File.ReadAllText(jsonFilePath);
//        }

<<<<<<< HEAD
//        public static List<string> AnalyzeJson(string jsonContent)
//        {
//            var parsedJson = JsonConvert.DeserializeObject(jsonContent);
//            var extractedData = new List<string>();
=======
        public static List<string> AnalyzeJson(string? jsonContent)
        {
            if (jsonContent == null)
            {
                throw new Exception("The provided JSON content is empty or malformed.");
            }

            var parsedJson = JsonConvert.DeserializeObject(jsonContent);
            var extractedData = new List<string>();
>>>>>>> origin/Development

//            if (parsedJson == null)
//            {
//                throw new Exception("The provided JSON content is empty or malformed.");
//            }

<<<<<<< HEAD
//            void Traverse(object? obj, string prefix = "")
//            {
//                if (obj is JObject jObject)
//                {
//                    foreach (var property in jObject.Properties())
//                    {
//                        Traverse(property.Value, $"{prefix}{property.Name}: ");
//                    }
//                }
//                else if (obj is JArray jArray)
//                {
//                    int index = 0;
//                    foreach (var item in jArray)
//                    {
//                        Traverse(item, $"{prefix}[{index++}]: ");
//                    }
//                }
//                else if (obj is JValue jValue)
//                {
//                    // Add the prefix and the value
//                    extractedData.Add($"{prefix.TrimEnd(' ')}{jValue.Value}");
//                }
//            }
=======
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
>>>>>>> origin/Development

//            Traverse(parsedJson);
//            return extractedData;
//        }
//    }

<<<<<<< HEAD
//    [TestClass]
//    public class JsonFileReaderUnitTest
//    {
//        [TestMethod]
//        public void ReadJsonFile_ValidFile_ReturnsContent()
//        {
//            // Arrange
//            string tempFilePath = "test.json";
//            string jsonContent = "{\"key\": \"value\"}";
//            File.WriteAllText(tempFilePath, jsonContent);

//            try
//            {
//                // Act
//                string result = Embedding.ReadJsonFile(tempFilePath);

//                // Assert
//                Assert.AreEqual(jsonContent, result, "The JSON content should match.");
//            }
//            finally
//            {
//                // Cleanup
//                File.Delete(tempFilePath);
//            }
//        }

//        [TestMethod]
//        public void ReadJsonFile_FileDoesNotExist_ThrowsFileNotFoundException()
//        {
//            // Arrange
//            string nonExistentFilePath = "nonexistent.json";

//            // Act & Assert
//            Assert.ThrowsException<FileNotFoundException>(() => Embedding.ReadJsonFile(nonExistentFilePath));
//        }
//    }

//    [TestClass]
//    public class JsonAnalyzerUnitTest
//    {
//        [TestMethod]
//        public void AnalyzeJson_ValidJson_ReturnsExtractedData()
//        {
//            // Arrange
//            string jsonContent = @"{
//                ""name"": ""John"",
//                ""age"": 30,
//                ""skills"": [""C#"", ""Python""]
//            }";

//            // Act
//            List<string> result = Embedding.AnalyzeJson(jsonContent);

//            // Assert
//            Assert.IsTrue(result.Contains("name: John"), "The name should be extracted.");
//            Assert.IsTrue(result.Contains("age: 30"), "The age should be extracted.");
//            Assert.IsTrue(result.Contains("skills[0]: C#"), "The first skill should be extracted.");
//            Assert.IsTrue(result.Contains("skills[1]: Python"), "The second skill should be extracted.");
//        }

//        [TestMethod]
//        public void AnalyzeJson_EmptyJson_ReturnsEmptyList()
//        {
//            // Arrange
//            string jsonContent = "{}";

//            // Act
//            List<string> result = Embedding.AnalyzeJson(jsonContent);

//            // Assert
//            Assert.AreEqual(0, result.Count, "The list should be empty for an empty JSON.");
//        }

//        [TestMethod]
//        public void AnalyzeJson_InvalidJson_ThrowsException()
//        {
//            // Arrange
//            string invalidJsonContent = "Invalid JSON";

//            // Act & Assert
//            Assert.ThrowsException<JsonReaderException>(() => Embedding.AnalyzeJson(invalidJsonContent));
//        }
//    }
//}
=======
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

        [TestMethod]
        public static string ReadJsonFile(string? jsonFilePath)
        {
            if (string.IsNullOrWhiteSpace(jsonFilePath))
            {
                throw new ArgumentException("File path cannot be null or whitespace.");
            }

            if (!File.Exists(jsonFilePath))
            {
                throw new FileNotFoundException($"The specified JSON file does not exist: {jsonFilePath}");
            }

            return File.ReadAllText(jsonFilePath);
        }

        [TestMethod]
        public void ReadJsonFile_WhitespaceFilePath_ShouldThrowArgumentException()
        {
            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => Embedding.ReadJsonFile(" "));
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
        public void AnalyzeJson_NullJson_ShouldThrowArgumentException()
        {
            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => Embedding.AnalyzeJson(null), "Should throw ArgumentException for null input.");
        }

        [TestMethod]
        public void AnalyzeJson_WhitespaceJson_ShouldThrowArgumentException()
        {
            // Arrange
            string whitespaceJson = "  ";

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => Embedding.AnalyzeJson(whitespaceJson), "Should throw ArgumentException for whitespace input.");
        }

        [TestMethod]
        public void AnalyzeJson_ArrayJson_ShouldExtractValues()
        {
            // Arrange
            string jsonArray = @"[ ""One"", ""Two"", ""Three"" ]";

            // Act
            List<string> result = Embedding.AnalyzeJson(jsonArray);

            // Assert
            CollectionAssert.AreEqual(new List<string> { "One", "Two", "Three" }, result, "Array elements should be extracted properly.");
        }
    }
}
>>>>>>> origin/Development
