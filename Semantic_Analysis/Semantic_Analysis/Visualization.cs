using ScottPlot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Semantic_Analysis
{
    public class Visualization
    {
        //public static void Main()
        //{
        //    try
        //    {
        //        IConfigurationRoot config = LoadConfiguration();
        //        string csvFile = config["FilePaths:CSVOutputFileName"];
        //        string scatterPlot = config["FilePaths:ScatterPlotOutputFile"];
        //        string rootDirectory = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName
        //            ?? throw new DirectoryNotFoundException("Could not determine root directory");
        //        string csvFilePath = Path.Combine(rootDirectory, config["FilePaths:OutputFolder"], csvFile);
        //        string outputImagePath = Path.Combine(rootDirectory, config["FilePaths:ScatterPlotFolder"], scatterPlot);
        //        Directory.CreateDirectory(Path.GetDirectoryName(outputImagePath));

        //        (List<double> xPositions, List<double> yValues, List<string> words, double documentSimilarity) = ProcessCsvData(csvFilePath);
        //        GenerateScatterPlot(xPositions, yValues, words, documentSimilarity, outputImagePath);

        //        Console.WriteLine($"Plot successfully saved to {outputImagePath}");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error: {ex.Message}");
        //        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        //    }
        //}

        public static IConfigurationRoot LoadConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        public static void GenerateScatterPlot(List<double> xPositions, List<double> yValues, List<string> words, double documentSimilarity, string outputPath)
        {
            var plt = new ScottPlot.Plot();

            for (int i = 0; i < xPositions.Count; i++)
            {
                var scatter = plt.Add.Scatter(new double[] { xPositions[i] }, new double[] { yValues[i] });
                scatter.Color = Colors.Blue;
                scatter.MarkerSize = 10;
                scatter.LineWidth = 0;

                var text = plt.Add.Text(words[i], xPositions[i], yValues[i]);
                text.LabelFontSize = 10;
                text.LabelFontColor = Colors.Black;
                text.Alignment = Alignment.UpperCenter;
                text.OffsetY = 10;
            }

            var referenceLine = plt.Add.HorizontalLine(0);
            referenceLine.Color = Colors.Black.WithAlpha(0.5f);
            referenceLine.LineWidth = 1f;
            referenceLine.LinePattern = LinePattern.Dashed;

            plt.XLabel("X Position");
            plt.YLabel("Cosine Similarity (-1 to 1)");
            //plt.Axes.SetLimitsY(-1, 1);

            var docSimText = plt.Add.Text($"Document Similarity: {documentSimilarity:F4}", (xPositions.Min() + xPositions.Max()) / 2, 1.05);
            docSimText.LabelFontSize = 14;
            docSimText.LabelFontColor = Colors.Black;
            docSimText.Alignment = Alignment.MiddleCenter;
            docSimText.LabelBold = true;

            plt.SavePng(outputPath, 1600, 900);
        }

        public static (List<double> xPositions, List<double> yValues, List<string> words, double documentSimilarity) ProcessCsvData(string csvFilePath)
        {
            if (!File.Exists(csvFilePath))
            {
                throw new FileNotFoundException($"CSV file not found at {csvFilePath}");
            }

            List<double> xPositions = new List<double>();
            List<double> yValues = new List<double>();
            List<string> words = new List<string>();
            double documentSimilarity = 0;

            var lines = File.ReadAllLines(csvFilePath);

            foreach (var line in lines)
            {
                if (line.StartsWith("Document_Similarity"))
                {
                    var parts = line.Split("-->", StringSplitOptions.TrimEntries);
                    if (parts.Length > 1 && double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double docSim))
                    {
                        documentSimilarity = docSim;
                    }
                    continue;
                }

                var columns = line.Split(",");
                if (columns.Length >= 4)
                {
                    string word1 = columns[0].Trim();
                    string xPosStr = columns[2].Trim();
                    string simStr = columns[3].Trim();

                    if (double.TryParse(xPosStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double xPosition) &&
                        double.TryParse(simStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double cosineSim))
                    {
                        xPositions.Add(xPosition);
                        yValues.Add(cosineSim);
                        words.Add(word1);
                    }
                }
            }

            if (xPositions.Count == 0)
            {
                throw new Exception("No valid data points found in CSV file");
            }

            return (xPositions, yValues, words, documentSimilarity);
        }
    }
}
