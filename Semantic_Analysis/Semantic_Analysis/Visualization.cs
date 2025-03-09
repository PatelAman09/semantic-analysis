using ScottPlot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Semantic_Analysis
{
    public class Visualization
    {
        static void Main()
        {
            try
            {
                // Load configuration from appsettings.json
                IConfigurationRoot config = LoadConfiguration();

                // Get file paths from configuration
                string csvFile = config["FilePaths:CSVOutputFileName"];
                string scatterPlot = config["FilePaths:ScatterPlotOutputFile"]; // Fixed space in key

                // Build complete file paths
                string rootDirectory = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName
                    ?? throw new DirectoryNotFoundException("Could not determine root directory");
                string csvFilePath = Path.Combine(rootDirectory, config["FilePaths:OutputFolder"], csvFile);
                string outputImagePath = Path.Combine(rootDirectory, config["FilePaths:ScatterPlotFolder"], scatterPlot);

                // Ensure directories exist
                Directory.CreateDirectory(Path.GetDirectoryName(outputImagePath));

                // Process CSV data
                (List<double> xPositions, List<double> yValues, List<string> words) = ProcessCsvData(csvFilePath);

                // Generate and save the plot
                GenerateScatterPlot(xPositions, yValues, words, outputImagePath);

                Console.WriteLine($"Plot successfully saved to {outputImagePath}");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"File not found: {ex.Message}");
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine($"Directory not found: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Detailed error reporting
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        // Method to load appsettings.json configuration
        public static IConfigurationRoot LoadConfiguration()
        {
            try
            {
                return new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load configuration: {ex.Message}", ex);
            }
        }

        // Process CSV data and extract required values
        private static (List<double> xPositions, List<double> yValues, List<string> words) ProcessCsvData(string csvFilePath)
        {
            if (!File.Exists(csvFilePath))
            {
                throw new FileNotFoundException($"CSV file not found at {csvFilePath}");
            }

            List<double> xPositions = new List<double>();
            List<double> yValues = new List<double>();
            List<string> words = new List<string>();

            var lines = File.ReadAllLines(csvFilePath);
            if (lines.Length <= 1)
            {
                throw new Exception("CSV file is empty or contains only a header row");
            }

            // Skip header row
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                var parts = line.Split(',');

                if (parts.Length < 6)
                {
                    Console.WriteLine($"Warning: Skipping malformed row {i + 1}: {line}");
                    continue;
                }

                if (double.TryParse(parts[4], out double xPosition) &&
                    double.TryParse(parts[5], NumberStyles.Float, CultureInfo.InvariantCulture, out double cosineSim))
                {
                    xPositions.Add(xPosition);
                    yValues.Add(2 * cosineSim - 1); // Scale cosine similarity to (-1, 1)
                    words.Add(parts[2].Trim('"')); // Extract word
                }
                else
                {
                    Console.WriteLine($"Warning: Could not parse numeric values in row {i + 1}");
                }
            }

            if (xPositions.Count == 0)
            {
                throw new Exception("No valid data points found in CSV file");
            }

            return (xPositions, yValues, words);
        }

        // Generate scatter plot with the extracted data - minimal version with no grid customization
        private static void GenerateScatterPlot(List<double> xPositions, List<double> yValues, List<string> words, string outputPath)
        {
            // Create a ScottPlot figure
            var plt = new ScottPlot.Plot();

            // Add scatter plot
            var scatter = plt.Add.Scatter(xPositions.ToArray(), yValues.ToArray());
            scatter.Color = Colors.Blue;
            scatter.MarkerSize = 5;
            scatter.LineWidth = 0; // Show only points, no connecting lines

            // Add reference line at y=0
            var referenceLine = plt.Add.HorizontalLine(0);
            referenceLine.Color = Colors.Black.WithAlpha(0.5);
            referenceLine.LineWidth = 1;
            referenceLine.LinePattern = LinePattern.Dashed;

            // Add labels using the simplest methods available
            plt.Title("Word Position vs. Cosine Similarity");
            plt.XLabel("Word Position (0 - 536)");
            plt.YLabel("Cosine Similarity (-1 to 1)");

            // Identify and highlight significant points
            // Simple approach to find points with highest absolute similarity values
            var significantIndices = new List<int>();

            // Create a list of (index, value) pairs for sorting
            var valueIndices = new List<(int index, double value)>();
            for (int i = 0; i < yValues.Count; i++)
            {
                valueIndices.Add((i, Math.Abs(yValues[i])));
            }

            // Sort by absolute y value (similarity) in descending order and take top 10
            var topIndices = valueIndices
                .OrderByDescending(pair => pair.value)
                .Take(10)
                .Select(pair => pair.index)
                .ToList();

            // Add text and highlight markers for significant points
            foreach (int idx in topIndices)
            {
                // Add text label
                var text = plt.Add.Text(words[idx], xPositions[idx], yValues[idx]);
                text.LabelFontSize = 12;
                text.LabelFontColor = Colors.Red;

                // Add highlight marker
                var highlight = plt.Add.Scatter(
                    new double[] { xPositions[idx] },
                    new double[] { yValues[idx] }
                );
                highlight.Color = Colors.Red;
                highlight.MarkerSize = 7;
            }

            // Set appropriate axis limits
            double xMin = xPositions.Min();
            double xMax = xPositions.Max();
            double yMin = yValues.Min();
            double yMax = yValues.Max();

            // Add padding
            double xPadding = (xMax - xMin) * 0.05;
            double yPadding = Math.Max(Math.Abs(yMin), Math.Abs(yMax)) * 0.1;

            // Use SetLimits with positional parameters only
            plt.Axes.SetLimits(
                xMin - xPadding,
                xMax + xPadding,
                Math.Min(-1, yMin - yPadding),
                Math.Max(1, yMax + yPadding)
            );

            // Ensure the output directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            // Save the plot
            plt.SavePng(outputPath, 1200, 800);
        }
    }
}