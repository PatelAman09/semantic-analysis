using Semantic_Analysis;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    [TestClass]
    public class DataExtractionTest
    {
        private DataExtraction? _dataExtraction;

        [TestInitialize]
        public void Setup()
        {
            _dataExtraction = new DataExtraction();
        }

        // Test for ExtractDataFromFile method (generic test for any supported file type)
        [TestMethod]
        public void ExtractDataFromFile_ShouldReturnNonEmptyData_WhenValidFilePath()
        {
            // Arrange
            string filePath = @"D:\IT\Software Engineering\SE Project\Saquib\semantic-analysis\Semantic_Analysis\Semantic_Analysis\Preprocessing\preprocessing.txt"; // Full file path

            // Check if the test file exists before proceeding
            if (!File.Exists(filePath))
            {
                Assert.Fail($"Test file not found at {filePath}. Please ensure the file exists before running the test.");
            }

            // Act
            var result = _dataExtraction!.ExtractDataFromFile(filePath);  // Using null-forgiving operator to tell compiler _dataExtraction is not null here

            // Assert
            Assert.IsTrue(result.Count > 0, "The file should contain at least one non-empty line of data.");
            foreach (var line in result)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(line), "There should be no empty or whitespace-only lines in the file.");
                Assert.IsTrue(line.Length > 2, "Each line should be meaningful (more than 2 characters).");
            }
        }

    }
}