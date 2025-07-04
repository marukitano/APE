using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpaceApiTest.Helper
{
    public class DynamicSpaceList
    {
        public static async Task RunAsync()
        {
            try
            {
                var url = "https://mapall.space/api.json";

                using var httpClient = new HttpClient();
                var jsonString = await httpClient.GetStringAsync(url);

                var jsonDoc = JsonDocument.Parse(jsonString);
                var root = jsonDoc.RootElement;

                if (!root.TryGetProperty("features", out var features) || features.ValueKind != JsonValueKind.Array)
                {
                    Console.WriteLine("❌ Keine Features gefunden.");
                    return;
                }

                // Alle möglichen Property-Namen sammeln
                var allKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var feature in features.EnumerateArray())
                {
                    if (feature.TryGetProperty("properties", out var props) && props.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in props.EnumerateObject())
                        {
                            allKeys.Add(prop.Name);
                        }
                    }
                }

                // Sortierte Liste für Ausgabe
                var sortedKeys = new List<string>(allKeys);
                sortedKeys.Sort(StringComparer.OrdinalIgnoreCase);

                Console.WriteLine("Alle gefundenen Properties (Felder):");
                Console.WriteLine(new string('-', 40));
                foreach (var key in sortedKeys)
                {
                    Console.WriteLine("- " + key);
                }

                Console.WriteLine("\n🔍 Ausgabe aller Spaces mit ALLEN Feldern:\n");

                foreach (var feature in features.EnumerateArray())
                {
                    if (!feature.TryGetProperty("properties", out var props) || props.ValueKind != JsonValueKind.Object)
                        continue;

                    Console.WriteLine("----- Hackerspace -----");

                    foreach (var key in sortedKeys)
                    {
                        string value = "";

                        if (props.ValueKind == JsonValueKind.Object && props.TryGetProperty(key, out var valProp))
                        {
                            value = valProp.ValueKind switch
                            {
                                JsonValueKind.String => valProp.GetString() ?? "",
                                JsonValueKind.Number => valProp.ToString(),
                                JsonValueKind.True => "true",
                                JsonValueKind.False => "false",
                                JsonValueKind.Null => "",
                                _ => valProp.ToString()
                            };
                        }

                        Console.WriteLine($"{key,-15}: {value}");
                    }

                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Fehler beim Laden oder Verarbeiten: " + ex.Message);
            }
        }
    }
}
