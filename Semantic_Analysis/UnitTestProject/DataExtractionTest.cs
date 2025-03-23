using System.Text.Json;
using Semantic_Analysis; 

namespace DataExtraction_UnitTest
{
    /// <summary>
    /// Unit Test Class for testing the DataExtraction functionality.
    /// </summary>
    [TestClass]
    public class DataExtractionTest
    {
        #region Fields
        /// <summary>
        /// Instance of DataExtraction class to be tested.
        /// </summary>
        private DataExtraction _dataExtraction;

        /// <summary>
        /// Path to the data preprocessing directory.
        /// </summary>
        private string? _dataPreprocessingPath;

        /// <summary>
        /// Path to the directory where extracted data will be saved.
        /// </summary>
        private string? _extractedDataPath;

        /// <summary>
        /// List of supported file extensions.
        /// </summary>
        private List<string>? _supportedFileExtensions;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor for the unit test class.
        /// Initializes the DataExtraction instance.
        /// </summary>
        public DataExtractionTest()
        {
            _dataExtraction = new DataExtraction(); // Initialize the DataExtraction instance.
        }
        #endregion

        #region Setup Method
        /// <summary>
        /// Setup method that runs before each test to initialize paths and validate configuration.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            // Load configuration settings from the appsettings.json file.
            var configuration = LoadConfiguration();

            // Retrieve paths from the configuration.
            _dataPreprocessingPath = configuration["FilePaths:DataPreprocessing"];
            _extractedDataPath = configuration["FilePaths:ExtractedData"];
            _supportedFileExtensions = configuration["FilePaths:SupportedFileExtensions"]?
                                              .Split(',') // Split extensions by comma.
                                              .Select(ext => ext.Trim().ToLower()) // Normalize the extensions to lowercase.
                                              .ToList() ?? new List<string>(); // If null, use an empty list.

            // Validate the loaded paths and supported file extensions.
            Assert.IsFalse(string.IsNullOrEmpty(_dataPreprocessingPath), "DataPreprocessing path should not be empty.");
            Assert.IsFalse(string.IsNullOrEmpty(_extractedDataPath), "ExtractedData path should not be empty.");
            Assert.IsTrue(_supportedFileExtensions?.Any() ?? false, "There should be at least one supported file extension.");

            // Find the solution root dynamically by looking for the .sln file in the directory structure.
            var solutionRoot = FindSolutionRoot(Directory.GetCurrentDirectory());

            if (string.IsNullOrEmpty(solutionRoot))
            {
                throw new InvalidOperationException("Solution root could not be determined.");
            }

            // Combine the solution root with the relative paths to ensure correct file paths.
            var rawDataPath = Path.Combine(solutionRoot, "Semantic_Analysis", _dataPreprocessingPath);
            var extractedDataPath = Path.Combine(solutionRoot, "Semantic_Analysis", _extractedDataPath);

            // Log paths for debugging.
            Console.WriteLine($"Solution Root: {solutionRoot}");
            Console.WriteLine($"DataPreprocessing Path: {rawDataPath}");
            Console.WriteLine($"ExtractedData Path: {extractedDataPath}");

            // Ensure the specified directories exist.
            Assert.IsTrue(Directory.Exists(rawDataPath), $"Directory does not exist: {rawDataPath}");
            Assert.IsTrue(Directory.Exists(extractedDataPath), $"Directory does not exist: {extractedDataPath}");

            // Assign the validated paths to class-level variables.
            _dataPreprocessingPath = rawDataPath;
            _extractedDataPath = extractedDataPath;
        }
        #endregion

        #region Helper Methods

        /// <summary>
        /// Method to find the root directory of the solution by looking for a .sln file.
        /// </summary>
        /// <param name="startingDirectory">The directory to start searching from.</param>
        /// <returns>The root directory containing the solution file or null if not found.</returns>
        private string? FindSolutionRoot(string startingDirectory)
        {
            var directory = new DirectoryInfo(startingDirectory);

            // Traverse up the directory tree until a solution file is found.
            while (directory != null)
            {
                var slnFile = directory.GetFiles("*.sln").FirstOrDefault();
                if (slnFile != null)
                {
                    return directory.FullName; // Return the directory containing the solution file.
                }

                // Move up to the parent directory.
                directory = directory.Parent;
            }

            return null; // Return null if no solution file is found.
        }

        /// <summary>
        /// Method to get a valid file from the specified directory.
        /// </summary>
        /// <param name="directoryPath">The directory path to search for files.</param>
        /// <returns>A valid file path if found; otherwise, an empty string.</returns>
        private string GetAnyFileFromDirectory(string? directoryPath)
        {
            // Validate the directory path.
            if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
            {
                Console.WriteLine($"Directory does not exist or is invalid: {directoryPath}");
                return string.Empty;
            }

            // Log the types of supported files we are searching for.
            Console.WriteLine($"Looking for supported file types: .txt, .csv, .json, .md, .xml, .html, .pdf, .docx");

            // Get all files in the directory and filter by supported extensions.
            var files = Directory.GetFiles(directoryPath)
                                 .Where(file =>
                                 {
                                     var extension = Path.GetExtension(file)?.ToLower();
                                     Console.WriteLine($"Checking file: {file} with extension {extension}");

                                     // Skip temporary files.
                                     if (Path.GetFileName(file).StartsWith("~$"))
                                     {
                                         return false;
                                     }

                                     // Check if the file's extension is supported.
                                     return _supportedFileExtensions?.Contains(extension, StringComparer.OrdinalIgnoreCase) ?? false;
                                 })
                                 .ToList();

            // Log the found files.
            Console.WriteLine($"Files found: {string.Join(", ", files ?? new List<string>())}");

            // Return the first valid file or empty string if no files are found.
            if (files == null || !files.Any())
            {
                Console.WriteLine($"No valid files found in the directory: {directoryPath}");
                return string.Empty;
            }

            return files.FirstOrDefault() ?? string.Empty;
        }

        /// <summary>
        /// Method to manually parse the appsettings.json file and load configuration.
        /// </summary>
        /// <returns>A dictionary containing the configuration values.</returns>
        private Dictionary<string, string> LoadConfiguration()
        {
            var configuration = new Dictionary<string, string>();

            // Define the path to the appsettings.json file.
            var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

            // Ensure the appsettings.json file exists.
            if (!File.Exists(appSettingsPath))
            {
                throw new FileNotFoundException("appsettings.json not found in the expected location.");
            }

            // Log the path and content of the appsettings.json file for debugging.
            Console.WriteLine($"Reading appsettings.json from: {appSettingsPath}");
            var jsonString = File.ReadAllText(appSettingsPath);
            Console.WriteLine($"appsettings.json content:\n{jsonString}");

            try
            {
                // Parse the JSON content of the appsettings.json.
                using (JsonDocument doc = JsonDocument.Parse(jsonString))
                {
                    var filePaths = doc.RootElement.GetProperty("FilePaths");

                    // Load the paths and supported file extensions from the JSON properties.
                    configuration["FilePaths:DataPreprocessing"] = filePaths.TryGetProperty("DataPreprocessing", out var dataPreprocessingProp)
                        ? dataPreprocessingProp.GetString() ?? string.Empty
                        : string.Empty;

                    configuration["FilePaths:ExtractedData"] = filePaths.TryGetProperty("ExtractedData", out var extractedDataProp)
                        ? extractedDataProp.GetString() ?? string.Empty
                        : string.Empty;

                    var extensionsArray = filePaths.GetProperty("SupportedFileExtensions").EnumerateArray();
                    var supportedExtensions = extensionsArray.Select(ext => ext.GetString()?.Trim().ToLower()).ToList();
                    configuration["FilePaths:SupportedFileExtensions"] = string.Join(",", supportedExtensions);
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error parsing appsettings.json: {ex.Message}");
                throw; // Rethrow to fail the test if configuration is invalid.
            }

            return configuration; // Return the loaded configuration as a dictionary.
        }
        #endregion

        #region Test Methods

        /// <summary>
        /// Test method to ensure data extraction works as expected from a valid file path.
        /// </summary>
        [TestMethod]
        public void ExtractDataFromFile_ShouldReturnNonEmptyData_WhenValidFilePath()
        {
            // Arrange: Retrieve a valid file from the DataPreprocessing directory.
            var filePath = GetAnyFileFromDirectory(_dataPreprocessingPath);

            // Log the file path for debugging purposes.
            Console.WriteLine($"File Path: {filePath}");

            // Check if the file path is valid and exists before proceeding.
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Assert.Fail($"No valid file found in the DataPreprocessing directory.");
            }

            try
            {
                // Ensure the DataExtraction instance is not null.
                Assert.IsNotNull(_dataExtraction, "DataExtraction is not initialized.");

                // Act: Perform the data extraction on the file.
                Console.WriteLine($"Starting data extraction for file: {filePath}");
                var result = _dataExtraction.ExtractDataFromFile(filePath);

                // Log the result of the data extraction for debugging.
                Console.WriteLine($"Data extracted: {string.Join(", ", result)}");

                // Assert: Ensure the extracted data is not empty.
                Assert.IsTrue(result?.Count > 0, "The file should contain at least one non-empty line of data.");

                // Validate each extracted line to ensure it is meaningful.
                foreach (var line in result)
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(line), "There should be no empty or whitespace-only lines in the file.");
                    Assert.IsTrue(line.Length > 2, "Each line should be meaningful (more than 2 characters).");
                }
            }
            catch (Exception ex)
            {
                // Fail the test if an exception occurs during the extraction process.
                Assert.Fail($"Data extraction failed for file {filePath}: {ex.Message}");
            }
        }
        #endregion
    }
}