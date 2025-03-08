using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using ScottPlot;
using ScottPlot.Panels;
using ScottPlot.Plottables;

namespace Semantic_Analysis
{
    public class HeatMapPlotter
    {
        public class HeatmapPlotter
        {
            public static void PlotHeatmap(string csvFilePath, string outputImagePath)
            {
                if (!File.Exists(csvFilePath))
                {
                    Console.WriteLine($"Error: CSV file '{csvFilePath}' not found.");
                    return;
                }

                List<(string word1, string word2, double similarity)> dataPoints = new();
                HashSet<string> wordSet = new();

                try
                {
                    string[] lines = File.ReadAllLines(csvFilePath);
                    if (lines.Length <= 1)
                    {
                        Console.WriteLine("Error: No data found in CSV.");
                        return;
                    }

                    // Read CSV data
                    foreach (var line in lines.Skip(1)) // Skip header
                    {
                        var parts = line.Split(',');
                        if (parts.Length < 4) continue;

                        string word1 = parts[0].Trim('"');
                        string word2 = parts[1].Trim('"');

                        if (double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out double similarity))
                        {
                            dataPoints.Add((word1, word2, similarity));
                            wordSet.Add(word1);
                            wordSet.Add(word2);
                        }
                    }

                    if (dataPoints.Count == 0)
                    {
                        Console.WriteLine("Error: No valid data for heatmap plotting.");
                        return;
                    }

                    // Generate unique Y-axis labels (mapping words to indices)
                    var wordList = wordSet.OrderBy(w => w).ToList();
                    Dictionary<string, int> wordIndex = wordList
                        .Select((word, index) => new { word, index })
                        .ToDictionary(x => x.word, x => x.index);

                    int wordCount = wordList.Count;
                    double[,] heatmapData = new double[wordCount, wordCount];

                    // Populate the heatmap matrix
                    foreach (var (word1, word2, similarity) in dataPoints)
                    {
                        int i = wordIndex[word1];
                        int j = wordIndex[word2];
                        heatmapData[i, j] = similarity;
                    }

                    // Initialize the Plot
                    var plt = new Plot();

                    // Create the Heatmap
                    var hm = new Heatmap(heatmapData)
                    {
                        Colormap = ScottPlot.Colormap.Turbo() // Use the Viridis colormap
                    };

                    // Add the Heatmap to the plot
                    plt.Add(hm);

                    // Set tick labels (Y-Axis and X-Axis)
                    plt.YTicks(wordList.ToArray());
                    plt.XTicks(wordList.ToArray());

                    // Set the size when saving the figure (in pixels)
                    plt.SaveFig(outputImagePath, 1500, 1000);  // Save the image with specified dimensions
                    Console.WriteLine($"Heatmap saved successfully at: {outputImagePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error generating heatmap: {ex.Message}");
                }
            }
        }
        public static void Main(string[] args)
        {
                // Load settings from appsettings.json
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                string csvFilePath = configuration["FilePaths:CSVInputFile"];
                string outputImagePath = configuration["FilePaths:HeatMapOutputFile"];

                if (string.IsNullOrEmpty(csvFilePath) || string.IsNullOrEmpty(outputImagePath))
                {
                    Console.WriteLine("Error: Invalid file paths in appsettings.json.");
                    return;
                }

                // Call HeatmapPlotter with file paths from configuration
                HeatmapPlotter.PlotHeatmap(csvFilePath, outputImagePath);
        }
    }
}

