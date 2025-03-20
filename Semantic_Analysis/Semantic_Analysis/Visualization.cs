using ScottPlot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace Semantic_Analysis
{
    public class Visualization
    {

        public static IConfigurationRoot LoadConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        public static void GenerateScatterPlot(List<double> xPositions, List<double> yValues, List<string> words, List<string> pairLabels, double documentSimilarity, string outputPath)
        {
            var plt = new ScottPlot.Plot();

            // Determine X-axis limits dynamically
            double xMin = xPositions.Min() - 50;
            double xMax = xPositions.Max() + 50;

            // Scatter plot with labels
            for (int i = 0; i < xPositions.Count; i++)
            {
                var scatter = plt.Add.Scatter(new double[] { xPositions[i] }, new double[] { yValues[i] });
                scatter.Color = Colors.Blue;
                scatter.MarkerSize = 10;
                scatter.LineWidth = 0;

                // Add text annotations
                var text = plt.Add.Text(pairLabels[i], xPositions[i], yValues[i]);
                text.LabelFontSize = 10;
                text.LabelFontColor = Colors.Black;
                text.Alignment = Alignment.UpperCenter;
                text.OffsetY = 10; // Adjust position
            }

            // Reference line at y=0
            var referenceLine = plt.Add.HorizontalLine(0);
            referenceLine.Color = Colors.Black.WithAlpha(0.5f);
            referenceLine.LineWidth = 1f;
            referenceLine.LinePattern = LinePattern.Dashed;

            // Update axis labels
            plt.XLabel("X Axis");
            plt.YLabel("Cosine Similarity (-1 to 1)");

            // Remove the default title and place document similarity at the top
            var docSimText = plt.Add.Text($"Document Similarity: {documentSimilarity:F4}", (xMin + xMax) / 2, 1.05);
            docSimText.LabelFontSize = 14;
            docSimText.LabelFontColor = Colors.Black;
            docSimText.Alignment = Alignment.MiddleCenter;
            docSimText.LabelBold = true;

            // Customize axis limits and ticks
            plt.Axes.SetLimits(xMin, xMax, -1.1, 1.1);
            plt.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericAutomatic();

            plt.SavePng(outputPath, 1600, 900);
        }

        public static (List<double> xPositions, List<double> yValues, List<string> words, List<string> pairLabels, double documentSimilarity) ProcessCsvData(string csvFilePath)
        {
            if (!File.Exists(csvFilePath))
            {
                throw new FileNotFoundException($"CSV file not found at {csvFilePath}");
            }

            List<double> xPositions = new List<double>();
            List<double> yValues = new List<double>();
            List<string> words = new List<string>();
            List<string> pairLabels = new List<string>();
            double documentSimilarity = 0;

            var lines = File.ReadAllLines(csvFilePath);

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];

                if (line.StartsWith("Document_Similarity"))
                {
                    var parts = line.Split("-->", StringSplitOptions.TrimEntries);
                    if (parts.Length > 1 && double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double docSim))
                    {
                        documentSimilarity = docSim;
                    }
                    continue;
                }

                MatchCollection matches = Regex.Matches(line, @"(?:^|,)(?=[^""]|("")?)""?((?(1)[^""]*|[^,""]*))""?(?=,|$)");

                if (matches.Count >= 6)
                {
                    string index1 = matches[0].Groups[2].Value.Trim('[', ']', ' ');
                    string index2 = matches[1].Groups[2].Value.Trim('[', ']', ' ');
                    string word1 = matches[2].Groups[2].Value.Trim('"');
                    string word2 = matches[3].Groups[2].Value.Trim('"');
                    string xPosStr = matches[4].Groups[2].Value.Trim();
                    string simStr = matches[5].Groups[2].Value.Trim();

                    if (double.TryParse(xPosStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double xPosition) &&
                        double.TryParse(simStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double cosineSim))
                    {
                        xPositions.Add(xPosition);
                        yValues.Add(cosineSim);

                        string truncatedWord1 = TruncateSentence(word1, 30);
                        string truncatedWord2 = TruncateSentence(word2, 30);
                        words.Add($"{truncatedWord1}...");

                        pairLabels.Add($"[{index1},{index2}]: {Math.Round(cosineSim, 2)}");
                    }
                }
            }

            if (xPositions.Count == 0)
            {
                throw new Exception("No valid data points found in CSV file");
            }

            return (xPositions, yValues, words, pairLabels, documentSimilarity);
        }

        public static string TruncateSentence(string sentence, int maxLength)
        {
            if (string.IsNullOrEmpty(sentence) || sentence.Length <= maxLength)
                return sentence;

            return sentence.Substring(0, maxLength);
        }
    }
}
