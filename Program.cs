using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonDifferenceFinder
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: JsonDifferenceFinder <file1> <file2>");
                return;
            }

            string file1Path = args[0];
            string file2Path = args[1];

            try
            {
                var json1 = File.ReadAllText(file1Path);
                var json2 = File.ReadAllText(file2Path);

                JToken obj1 = JToken.Parse(json1);
                JToken obj2 = JToken.Parse(json2);

                var differences = FindDifferences(obj1, obj2);

                Console.WriteLine("Differences between the two JSON files:");
                foreach (var diff in differences)
                {
                    Console.WriteLine(diff);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        static List<string> FindDifferences(JToken obj1, JToken obj2, string path = "")
        {
            var differences = new List<string>();

            if (obj1.Type != obj2.Type)
            {
                differences.Add($"Type mismatch at path '{path}': {obj1.Type} vs {obj2.Type}");
                return differences;
            }

            if (obj1 is JObject obj1Obj && obj2 is JObject obj2Obj)
            {
                foreach (var property in obj1Obj.Properties())
                {
                    var newPath = string.IsNullOrEmpty(path) ? property.Name : $"{path}.{property.Name}";
                    if (!obj2Obj.ContainsKey(property.Name))
                    {
                        differences.Add($"Missing property in second JSON at path '{newPath}'");
                    }
                    else
                    {
                        differences.AddRange(FindDifferences(property.Value, obj2Obj[property.Name], newPath));
                    }
                }

                foreach (var property in obj2Obj.Properties())
                {
                    var newPath = string.IsNullOrEmpty(path) ? property.Name : $"{path}.{property.Name}";
                    if (!obj1Obj.ContainsKey(property.Name))
                    {
                        differences.Add($"Missing property in first JSON at path '{newPath}'");
                    }
                }
            }
            else if (obj1 is JArray obj1Arr && obj2 is JArray obj2Arr)
            {
                int maxLength = Math.Max(obj1Arr.Count, obj2Arr.Count);
                for (int i = 0; i < maxLength; i++)
                {
                    var newPath = $"{path}[{i}]";
                    if (i >= obj1Arr.Count)
                    {
                        differences.Add($"Missing element in first JSON at path '{newPath}'");
                    }
                    else if (i >= obj2Arr.Count)
                    {
                        differences.Add($"Missing element in second JSON at path '{newPath}'");
                    }
                    else
                    {
                        differences.AddRange(FindDifferences(obj1Arr[i], obj2Arr[i], newPath));
                    }
                }
            }
            else if (!JToken.DeepEquals(obj1, obj2))
            {
                differences.Add($"Value mismatch at path '{path}': {obj1} vs {obj2}");
            }

            return differences;
        }
    }
}
