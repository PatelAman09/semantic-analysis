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

//        public static List<string> AnalyzeJson(string jsonContent)
//        {
//            var parsedJson = JsonConvert.DeserializeObject(jsonContent);
//            var extractedData = new List<string>();

//            if (parsedJson == null)
//            {
//                throw new Exception("The provided JSON content is empty or malformed.");
//            }

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

//            Traverse(parsedJson);
//            return extractedData;
//        }
//    }

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
