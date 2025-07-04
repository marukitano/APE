using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace HackerspaceLogic.Helper
{
    public class DynamicPropertyScanner
    {
        public static async Task RunAsync()
        {
            string apiUrl = "https://mapall.space/api.json";

            try
            {
                using HttpClient client = new();
                string json = await client.GetStringAsync(apiUrl);

                using JsonDocument doc = JsonDocument.Parse(json);

                Dictionary<string, Dictionary<JsonValueKind, int>> typeStats = new();

                if (doc.RootElement.TryGetProperty("features", out JsonElement features))
                {
                    foreach (JsonElement feature in features.EnumerateArray())
                    {
                        if (feature.TryGetProperty("properties", out JsonElement properties))
                        {
                            foreach (JsonProperty prop in properties.EnumerateObject())
                            {
                                var name = prop.Name;
                                var kind = prop.Value.ValueKind;

                                if (!typeStats.ContainsKey(name))
                                    typeStats[name] = new Dictionary<JsonValueKind, int>();

                                if (!typeStats[name].ContainsKey(kind))
                                    typeStats[name][kind] = 0;

                                typeStats[name][kind]++;
                            }
                        }
                    }
                }

                Console.WriteLine("Alle möglichen Felder mit allen Typen & Häufigkeit:");
                Console.WriteLine(new string('-', 70));

                foreach (var field in SortedByKey(typeStats))
                {
                    Console.Write($"{field.Key,-20} => ");

                    List<string> typeCounts = new();

                    foreach (var kind in field.Value)
                    {
                        typeCounts.Add($"{kind.Key} ({kind.Value}x)");
                    }

                    Console.WriteLine(string.Join(", ", typeCounts));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Fehler: {ex.Message}");
            }
        }

        private static IEnumerable<KeyValuePair<string, Dictionary<JsonValueKind, int>>> SortedByKey(Dictionary<string, Dictionary<JsonValueKind, int>> dict)
        {
            var list = new List<KeyValuePair<string, Dictionary<JsonValueKind, int>>>(dict);
            list.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.OrdinalIgnoreCase));
            return list;
        }
    }
}
