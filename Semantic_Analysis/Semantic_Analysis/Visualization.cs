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

        // Generate scatter plot with the extracted data - minimal version with no grid customization
        private static void GenerateScatterPlot(List<double> xPositions, List<double> yValues, List<string> words, string outputPath)
        {
            // Create a ScottPlot figure
            var plt = new ScottPlot.Plot();

            // Add scatter plot
            var scatter = plt.Add.Scatter(xPositions.ToArray(), yValues.ToArray());
            scatter.Color = Colors.Blue;
            scatter.MarkerSize = 5f;
            scatter.LineWidth = 0f;

            // Add reference line at y=0
            var referenceLine = plt.Add.HorizontalLine(0);
            referenceLine.Color = Colors.Black.WithAlpha(0.5f);
            referenceLine.LineWidth = 1f;
            referenceLine.LinePattern = LinePattern.Dashed;

            // Customize grid appearance
            plt.Grid.MajorLineColor = Colors.Gray.WithAlpha(0.2f);
            plt.Grid.MinorLineColor = Colors.Gray.WithAlpha(0.1f);
            plt.Grid.MajorLineWidth = 1f;
            plt.Grid.MinorLineWidth = 0.5f;

            // Add labels
            plt.Title("Word Position vs. Cosine Similarity");
            plt.XLabel("Word Position (0 - 536)");
            plt.YLabel("Cosine Similarity (-1 to 1)");

            // Instead of selecting top 10, display all points from CSV since there are only a few
            for (int i = 0; i < xPositions.Count; i++)
            {
                // Add highlight marker for all points
                var highlight = plt.Add.Scatter(
                    new double[] { xPositions[i] },
                    new double[] { yValues[i] }
                );
                highlight.Color = Colors.Red;
                highlight.MarkerSize = 7f;

                // Add text label with optimized positioning
                var text = plt.Add.Text(words[i], xPositions[i], yValues[i]);
                text.LabelFontSize = 10f;
                text.LabelFontColor = Colors.Red;

                // Adjust text positioning to avoid truncation
                if (xPositions[i] > 200) // For right side of plot
                {
                    text.Alignment = Alignment.MiddleRight;
                    text.OffsetX = -10f;
                    text.OffsetY = 10f;

                    // Split long text into multiple lines if needed
                    if (words[i].Length > 30)
                    {
                        // Try to place the text in a better position for readability
                        text.OffsetY = -15f; // Place above the point
                    }
                }
                else // For left side of plot
                {
                    text.Alignment = Alignment.MiddleLeft;
                    text.OffsetX = 10f;
                    text.OffsetY = 10f;

                    // Split long text into multiple lines if needed
                    if (words[i].Length > 30)
                    {
                        // Try to place the text in a better position for readability
                        text.OffsetY = -15f; // Place above the point
                    }
                }
            }

            // Set appropriate axis limits
            // We only need to show 0-270 range based on the data
            double xMin = -10; // Slight padding on left
            double xMax = 280; // Slight padding on right to accommodate point at 268

            // Force Y axis to show -1 to 1 range
            plt.Axes.SetLimits(xMin, xMax, -1, 1);

            // Ensure the output directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            // Save the plot with larger dimensions
            plt.SavePng(outputPath, 1920, 1080);
        }

        // Also update the ProcessCsvData method to handle the specific CSV format
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

            // Skip the last line which is the header (reversed in this file)
            for (int i = 0; i < lines.Length - 1; i++)
            {
                var line = lines[i];
                var parts = line.Split(',');

                // The CSV format shows: [index1],[index2],"text1","text2",x-position,similarity
                if (parts.Length >= 6)
                {
                    // Extract x position which is the 5th element (0-indexed)
                    if (double.TryParse(parts[4], out double xPosition))
                    {
                        // Extract cosine similarity which is the 6th element
                        if (double.TryParse(parts[5], NumberStyles.Float, CultureInfo.InvariantCulture, out double cosineSim))
                        {
                            xPositions.Add(xPosition);

                            // Scale value to -1 to 1 range if needed (depends on if already scaled)
                            double scaledValue = (cosineSim > 1) ? (2 * cosineSim - 1) : cosineSim;
                            yValues.Add(scaledValue);

                            // Extract the first text as the word label (between first set of quotes)
                            string text = ExtractTextBetweenQuotes(line);

                            // Truncate very long texts for display purposes
                            if (text.Length > 50)
                            {
                                text = text.Substring(0, 47) + "...";
                            }

                            words.Add(text);
                        }
                    }
                }
            }

            if (xPositions.Count == 0)
            {
                throw new Exception("No valid data points found in CSV file");
            }

            return (xPositions, yValues, words);
        }

        // Helper method to extract text between quotes
        private static string ExtractTextBetweenQuotes(string line)
        {
            int firstQuote = line.IndexOf('"');
            if (firstQuote >= 0)
            {
                int secondQuote = line.IndexOf('"', firstQuote + 1);
                if (secondQuote >= 0)
                {
                    return line.Substring(firstQuote + 1, secondQuote - firstQuote - 1);
                }
            }
            return string.Empty;
        }
    }
}