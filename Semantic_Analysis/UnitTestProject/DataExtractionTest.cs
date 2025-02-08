using Semantic_Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProject
{
    [TestClass]
    public class DataExtractionTest
    {
        private DataExtraction _dataExtraction = new DataExtraction();

        [TestInitialize]
        public void Setup()
        {
            _dataExtraction = new DataExtraction();
        }

        // Test for ExtractDataFromFile method
        [TestMethod]
        public void ExtractDataFromFile_ShouldReturnNonEmptyData_WhenValidFilePath()
        {
            // Arrange
            string filePath = @"D:\IT\Software Engineering\SE Project\Saquib\semantic-analysis\Semantic_Analysis\Semantic_Analysis\Preprocessing\preprocessing.txt"; // Full file path

            // Debugging: Output the file path to ensure it's correct
            Console.WriteLine($"Checking for file at: {filePath}");
            Console.WriteLine($"File exists: {File.Exists(filePath)}");

            // Check if the test file exists before proceeding
            if (!File.Exists(filePath))
            {
                Assert.Fail($"Test file not found at {filePath}. Please ensure the file exists before running the test.");
            }

            // Act
            var result = _dataExtraction.ExtractDataFromFile(filePath);

            // Debugging: Output the actual result to compare with the expected data
            Console.WriteLine("Extracted Data:");
            foreach (var line in result)
            {
                Console.WriteLine(line);
            }

            // Assert: Ensure the result contains data
            Assert.IsTrue(result.Count > 0, "The file should contain at least one non-empty line of data.");

            // Assert: Ensure all lines are non-empty and have a minimum length
            foreach (var line in result)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(line), "There should be no empty or whitespace-only lines in the file.");
                Assert.IsTrue(line.Length > 2, "Each line should be meaningful (more than 2 characters).");
            }
        }

        [TestMethod]
        public void ExtractDataFromText_ShouldReturnNonEmptyData_WhenValidFilePath()
        {
            // Arrange
            string filePath = @"D:\IT\Software Engineering\SE Project\Saquib\semantic-analysis\Semantic_Analysis\Semantic_Analysis\Preprocessing\preprocessing.txt"; 

            if (!File.Exists(filePath))
            {
                Assert.Fail($"Test file not found at {filePath}. Please ensure the file exists before running the test.");
            }

            // Act
            var result = _dataExtraction.ExtractDataFromText(filePath);

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
