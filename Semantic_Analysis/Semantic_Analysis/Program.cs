using Microsoft.Extensions.Configuration;
using Semantic_Analysis;
using Semantic_Analysis.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Semantic_Analysis
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("=== Semantic Analysis Workflow ===");
                Console.WriteLine("1. Data Extraction");
                Console.WriteLine("2. Embedding Generation");
                Console.WriteLine("3. Cosine Similarity Calculation");
                Console.WriteLine("4. Visualization");
                Console.WriteLine("================================");

                // Load configuration
                var configuration = DataExtraction.LoadConfiguration();
                string rootDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;

                // Step 1: Data Extraction
                await ExecuteDataExtractionStepAsync(configuration, rootDirectory);

                // Step 2: Embedding Generation
                await ExecuteEmbeddingGenerationStepAsync(configuration, rootDirectory);

                // Step 3: Cosine Similarity Calculation
                await ExecuteCosineSimilarityStepAsync(configuration, rootDirectory);

                // Step 4: Visualization
                ExecuteVisualizationStep(configuration, rootDirectory);

                Console.WriteLine("Semantic Analysis workflow completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Semantic Analysis workflow: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }



        private static async Task ExecuteDataExtractionStepAsync(IConfiguration configuration, string rootDirectory)
        {
            Console.WriteLine("\nExecuting Step 1: Data Extraction...");

            // Retrieve folder paths from configuration
            string dataPreprocessingPath = configuration["FilePaths:DataPreprocessing"];
            string extractedDataPath = configuration["FilePaths:ExtractedData"];

            // Get supported file extensions
            var supportedExtensions = configuration.GetSection("FilePaths:SupportedFileExtensions")
                                                 .AsEnumerable()
                                                 .Where(x => x.Value != null)
                                                 .Select(x => x.Value)
                                                 .ToList();

            // Resolve absolute paths
            string absoluteDataPreprocessingPath = Path.Combine(rootDirectory, dataPreprocessingPath);
            string absoluteExtractedDataPath = Path.Combine(rootDirectory, extractedDataPath);

            // Ensure directories exist
            DataExtraction.EnsureDirectoryExists(absoluteExtractedDataPath);

            // Get files with supported extensions
            var filesInRawData = Directory.GetFiles(absoluteDataPreprocessingPath)
                                        .Where(file => supportedExtensions.Contains(Path.GetExtension(file).ToLower()))
                                        .ToList();

            // Validate file count
            if (filesInRawData.Count != 2)
            {
                throw new Exception($"Error: Expected exactly two files in {absoluteDataPreprocessingPath}, but found {filesInRawData.Count}.");
            }

            // Process the files
            string extractedDataFilePath = filesInRawData[0];
            string referenceDocumentFilePath = filesInRawData[1];

            // Define output paths
            string outputExtractedDataFilePath = Path.Combine(absoluteExtractedDataPath, $"{Path.GetFileNameWithoutExtension(extractedDataFilePath)}.json");
            string outputReferenceDocumentFilePath = Path.Combine(absoluteExtractedDataPath, $"{Path.GetFileNameWithoutExtension(referenceDocumentFilePath)}.json");

            // Create an instance of DataExtraction
            IDataExtraction processor = new DataExtraction();

            // Process extracted data file (using Task.Run for CPU-bound work)
            Console.WriteLine($"Processing file: {Path.GetFileName(extractedDataFilePath)}");
            List<string> extractedData = await Task.Run(() => processor.ExtractDataFromFile(extractedDataFilePath));
            List<string> cleanedExtractedData = await Task.Run(() => processor.CleanData(extractedData));
            await Task.Run(() => processor.SaveDataToJson(outputExtractedDataFilePath, cleanedExtractedData, "extracted"));

            // Process reference document file (using Task.Run for CPU-bound work)
            Console.WriteLine($"Processing file: {Path.GetFileName(referenceDocumentFilePath)}");
            List<string> referenceData = await Task.Run(() => processor.ExtractDataFromFile(referenceDocumentFilePath));
            List<string> cleanedReferenceData = await Task.Run(() => processor.CleanData(referenceData));
            await Task.Run(() => processor.SaveDataToJson(outputReferenceDocumentFilePath, cleanedReferenceData, "reference"));

            // Output the result of the data extraction
            Console.WriteLine($"Data extracted and saved to: {outputExtractedDataFilePath}");
            Console.WriteLine($"Reference document data extracted and saved to: {outputReferenceDocumentFilePath}");
            Console.WriteLine("Data extraction completed successfully.");
        }

        private static async Task ExecuteEmbeddingGenerationStepAsync(IConfiguration configuration, string rootDirectory)
        {
            Console.WriteLine("\nExecuting Step 2: Embedding Generation...");

            // Get API key from environment variables
            string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("Environment variable 'OPENAI_API_KEY' is not set.");
            }

            // Get folder paths from configuration
            string inputFolder = Path.Combine(rootDirectory, configuration["FilePaths:ExtractedData"]);
            string outputFolder = Path.Combine(rootDirectory, configuration["FilePaths:InputFolder"]);

            // Define output file paths
            string outputFile1 = Path.Combine(outputFolder, configuration["FilePaths:InputFileName1"]);
            string outputFile2 = Path.Combine(outputFolder, configuration["FilePaths:InputFileName2"]);

            // Ensure output directory exists
            DataExtraction.EnsureDirectoryExists(outputFolder);

            // Clean up previous output files
            if (File.Exists(outputFile1)) File.Delete(outputFile1);
            if (File.Exists(outputFile2)) File.Delete(outputFile2);

            // Get JSON files from input folder
            var jsonFiles = Directory.GetFiles(inputFolder, "*.json");
            if (jsonFiles.Length < 2)
            {
                throw new Exception($"Expected at least two JSON files in {inputFolder}, but found {jsonFiles.Length}.");
            }

            Console.WriteLine($"Processing files: {Path.GetFileName(jsonFiles[0])} and {Path.GetFileName(jsonFiles[1])}");

            // Create embedding processor instance
            IEmbeddingProcessor processor = new EmbeddingProcessor();

            // Process both files in parallel
            await Task.WhenAll(
                processor.ProcessJsonFileAsync(jsonFiles[0], outputFile1, apiKey, 10),
                processor.ProcessJsonFileAsync(jsonFiles[1], outputFile2, apiKey, 10)
            );

            Console.WriteLine("Embedding generation completed successfully.");
        }

        private static async Task ExecuteCosineSimilarityStepAsync(IConfiguration configuration, string rootDirectory)
        {
            Console.WriteLine("\nExecuting Step 3: Cosine Similarity Calculation...");

            // Get file paths from configuration
            string inputFile1 = configuration["FilePaths:InputFileName1"];
            string inputFile2 = configuration["FilePaths:InputFileName2"];
            string outputFileName = configuration["FilePaths:CSVOutputFileName"];

            // Resolve absolute paths
            string inputFilePath1 = Path.Combine(rootDirectory, configuration["FilePaths:InputFolder"], inputFile1);
            string inputFilePath2 = Path.Combine(rootDirectory, configuration["FilePaths:InputFolder"], inputFile2);
            string outputFilePath = Path.Combine(rootDirectory, configuration["FilePaths:OutputFolder"], outputFileName);

            // Ensure output directory exists
            string outputFolder = Path.Combine(rootDirectory, configuration["FilePaths:OutputFolder"]);
            DataExtraction.EnsureDirectoryExists(outputFolder);

            // Create cosine similarity calculator instance
            ICosineSimilarity cosineSimilarity = new CosineSimilarity();

            try
            {
                // Read vectors from CSV files (using Task.Run for I/O operations)
                Console.WriteLine($"Reading vectors from: {Path.GetFileName(inputFilePath1)}");
                Dictionary<string, (string text, double[] vector)> vectorsFile1 =
                    await Task.Run(() => cosineSimilarity.ReadVectorsFromCsv(inputFilePath1));

                Console.WriteLine($"Reading vectors from: {Path.GetFileName(inputFilePath2)}");
                Dictionary<string, (string text, double[] vector)> vectorsFile2 =
                    await Task.Run(() => cosineSimilarity.ReadVectorsFromCsv(inputFilePath2));

                // Validate vectors
                if (vectorsFile1.Count == 0 || vectorsFile2.Count == 0)
                {
                    throw new InvalidOperationException("Each input file must contain at least one valid vector.");
                }

                await Task.Run(() => cosineSimilarity.ValidateVectors(vectorsFile1));
                await Task.Run(() => cosineSimilarity.ValidateVectors(vectorsFile2));

                // Prepare output data
                List<string> outputData = new List<string>();
                outputData.Add("Index1,Index2,Word1,Word2,X_Position,Cosine_Similarity");

                // Get ordered indices for consistent x-axis positioning
                List<string> orderedIndices1 = vectorsFile1.Keys.OrderBy(k => k).ToList();

                // Calculate step size for x-axis positioning
                double xAxisRange = 536.0;
                double stepSize = (orderedIndices1.Count > 1) ? xAxisRange / (orderedIndices1.Count - 1) : xAxisRange / 2;


                // Process each pair of vectors (using Task.Run for CPU-bound calculations)
                Console.WriteLine("Calculating cosine similarities for all pairs of vectors...");

                var calculationTasks = new List<Task<string>>();

                for (int i = 0; i < orderedIndices1.Count; i++)
                {
                    string index1 = orderedIndices1[i];
                    int xPosition = (int)(i * stepSize);

                    foreach (var index2 in vectorsFile2.Keys)
                    {
                        // Capture values for lambda
                        string capturedIndex1 = index1;
                        string capturedIndex2 = index2;
                        int capturedXPosition = xPosition;

                        calculationTasks.Add(Task.Run(() => {
                            double similarity = Math.Round(
                                cosineSimilarity.CosineSimilarityCalculation(
                                    vectorsFile1[capturedIndex1].vector,
                                    vectorsFile2[capturedIndex2].vector
                                ),
                                10
                            );
                            return $"{capturedIndex1},{capturedIndex2},\"{vectorsFile1[capturedIndex1].text}\",\"{vectorsFile2[capturedIndex2].text}\",{capturedXPosition},{similarity.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
                        }));
                    }
                }

                // Wait for all calculation tasks to complete
                var results = await Task.WhenAll(calculationTasks);
                outputData.AddRange(results);

                // Calculate overall document similarity
                double documentSimilarity = await Task.Run(() =>
                    cosineSimilarity.CalculateDocumentSimilarity(vectorsFile1, vectorsFile2));

                Console.WriteLine($"Overall document similarity: {documentSimilarity:F4}");

                // Add document similarity to output
                outputData.Add($"Document_Similarity --> {documentSimilarity.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

                // Save results to CSV
                await Task.Run(() => cosineSimilarity.SaveOutputToCsv(outputFilePath, outputData));

                Console.WriteLine($"Successfully processed {vectorsFile1.Count} words from file 1 and {vectorsFile2.Count} words from file 2.");
                Console.WriteLine($"Results saved to: {outputFilePath}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in cosine similarity calculation: {ex.Message}", ex);
            }
        }

        private static void ExecuteVisualizationStep(IConfiguration configuration, string rootDirectory)
        {
            Console.WriteLine("\nExecuting Step 4: Visualization...");
            try
            {
                string csvFile = configuration["FilePaths:CSVOutputFileName"];
                string scatterPlot = configuration["FilePaths:ScatterPlotOutputFile"];
                string csvFilePath = Path.Combine(rootDirectory, configuration["FilePaths:OutputFolder"], csvFile);
                string outputImagePath = Path.Combine(rootDirectory, configuration["FilePaths:ScatterPlotFolder"], scatterPlot);
                Directory.CreateDirectory(Path.GetDirectoryName(outputImagePath));

                var (xPositions, yValues, words, pairLabels, documentSimilarity) = Visualization.ProcessCsvData(csvFilePath);

                Visualization.GenerateScatterPlot(
                    xPositions, // Assuming these are now static properties
                    yValues,
                    words,
                    pairLabels,
                    documentSimilarity,
                    outputImagePath);

                Console.WriteLine($"Plot successfully saved to {outputImagePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}