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
        //public static void Main()
        //{
        //    try
        //    {
        //        // Load configuration from appsettings.json
        //        IConfigurationRoot config = LoadConfiguration();

        //        // Get file paths from configuration
        //        string csvFile = config["FilePaths:CSVOutputFileName"];
        //        string scatterPlot = config["FilePaths:ScatterPlotOutputFile"]; // Fixed space in key

        //        // Build complete file paths
        //        string rootDirectory = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName
        //            ?? throw new DirectoryNotFoundException("Could not determine root directory");
        //        string csvFilePath = Path.Combine(rootDirectory, config["FilePaths:OutputFolder"], csvFile);
        //        string outputImagePath = Path.Combine(rootDirectory, config["FilePaths:ScatterPlotFolder"], scatterPlot);

        //        // Ensure directories exist
        //        Directory.CreateDirectory(Path.GetDirectoryName(outputImagePath));

        //        // Process CSV data
        //        (List<double> xPositions, List<double> yValues, List<string> words) = ProcessCsvData(csvFilePath);

        //        // Generate and save the plot
        //        GenerateScatterPlot(xPositions, yValues, words, outputImagePath);

        //        Console.WriteLine($"Plot successfully saved to {outputImagePath}");
        //    }
        //    catch (FileNotFoundException ex)
        //    {
        //        Console.WriteLine($"File not found: {ex.Message}");
        //    }
        //    catch (DirectoryNotFoundException ex)
        //    {
        //        Console.WriteLine($"Directory not found: {ex.Message}");
        //    }
        //    catch (Exception ex)
        //    {
        //        // Detailed error reporting
        //        Console.WriteLine($"Error: {ex.Message}");
        //        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        //    }
        //}

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

                if (xPositions[i] == 0)
                {
                    text.Alignment = Alignment.MiddleLeft;
                    text.OffsetX = 15f; // Ensure text is not hidden at axis edge
                    text.OffsetY = 10f;
                }

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
            double xMin = 0;
            double xMax = 536;

            // Force Y axis to show -1 to 1 range
            plt.Axes.SetLimits(xMin, xMax, -1, 1);

            // Ensure the output directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            // Save the plot with larger dimensions
            plt.SavePng(outputPath, 1600, 900);
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
                    // Make sure to handle parsing correctly by trimming any quotes
                    string xPosStr = parts[4].Trim('"', ' ');
                    string simStr = parts[5].Trim('"', ' ');

                    if (double.TryParse(xPosStr, out double xPosition))
                    {
                        if (double.TryParse(simStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double cosineSim))
                        {
                            xPositions.Add(xPosition);
                            yValues.Add(cosineSim); // Use raw value without scaling

                            // Extract label text
                            string text = ExtractTextBetweenQuotes(line);
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
            var matches = System.Text.RegularExpressions.Regex.Matches(line, "\"(.*?)\"");
            return matches.Count > 1 ? matches[1].Value : string.Empty; // Extract second quoted field
        }
    }
}