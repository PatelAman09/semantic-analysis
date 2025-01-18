using System;
using System.IO;

namespace YourNamespace
{
    public static class JsonFileReader
    {
        public static string ReadJsonFile(string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath))
            {
                throw new FileNotFoundException($"The specified JSON file does not exist: {jsonFilePath}");
            }

            return File.ReadAllText(jsonFilePath);
        }
    }
}