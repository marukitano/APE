using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HackerspaceLogic.Helper
{
    public class RemoteSourceScanner
    {
        public static async Task RunAsync()
        {
            string apiUrl = "https://mapall.space/api.json";
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };

            var globalStats = new Dictionary<string, Dictionary<JsonValueKind, int>>();
            var failedSpaces = new List<(string Name, string Email, string Url, string Error)>();
            int total = 0, success = 0;

            try
            {
                string rootJson = await client.GetStringAsync(apiUrl);
                using JsonDocument doc = JsonDocument.Parse(rootJson);

                if (!doc.RootElement.TryGetProperty("features", out var features) || features.ValueKind != JsonValueKind.Array)
                {
                    Console.WriteLine("Keine 'features'-Liste gefunden.");
                    return;
                }

                var sourceInfos = new List<(string Url, string Name, string Email)>();

                foreach (var feature in features.EnumerateArray())
                {
                    if (feature.TryGetProperty("properties", out var props))
                    {
                        string name = props.TryGetProperty("name", out var n) ? n.GetString() ?? "???" : "???";
                        string email = props.TryGetProperty("email", out var e) ? e.GetString() ?? "-" : "-";
                        string sourceUrl = props.TryGetProperty("source", out var s) ? s.GetString() ?? "" : "";

                        if (!string.IsNullOrWhiteSpace(sourceUrl))
                        {
                            sourceInfos.Add((sourceUrl, name, email));
                        }
                    }
                }

                total = sourceInfos.Count;
                Console.WriteLine($"Prüfe {total} JSON-Quellen...\n");

                int current = 0;
                foreach (var (url, name, email) in sourceInfos)
                {
                    current++;
                    Console.Write($"\rVerarbeite JSON {current} / {total} ...".PadRight(40));

                    try
                    {
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                        string json = await client.GetStringAsync(url, cts.Token);
                        using var spaceDoc = JsonDocument.Parse(json);
                        var root = spaceDoc.RootElement;

                        foreach (var prop in root.EnumerateObject())
                        {
                            var propName = prop.Name;
                            var kind = prop.Value.ValueKind;

                            if (!globalStats.ContainsKey(propName))
                                globalStats[propName] = new();

                            if (!globalStats[propName].ContainsKey(kind))
                                globalStats[propName][kind] = 0;

                            globalStats[propName][kind]++;
                        }

                        success++;
                    }
                    catch (Exception ex)
                    {
                        failedSpaces.Add((name, email, url, ex.Message));
                    }
                }

                Console.WriteLine(); // Zeilenumbruch nach Fortschrittsanzeige

                Console.WriteLine("\nGefundene Felder aus verlinkten source-JSONs:\n");

                foreach (var entry in SortedByKey(globalStats))
                {
                    Console.Write($"{entry.Key,-25} => ");
                    List<string> typeInfo = new();

                    foreach (var kv in entry.Value)
                    {
                        typeInfo.Add($"{kv.Key} ({kv.Value}x)");
                    }

                    Console.WriteLine(string.Join(", ", typeInfo));
                }

                Console.WriteLine("\nZusammenfassung:");
                Console.WriteLine(new string('-', 50));
                Console.WriteLine($"Insgesamt: {total}");
                Console.WriteLine($"Erfolgreich geladen: {success}");
                Console.WriteLine($"Fehlerhafte Quellen: {failedSpaces.Count}");

                if (failedSpaces.Count > 0)
                {
                    Console.Write("\nDrücke [D] für Details oder beliebige andere Taste zum Beenden: ");
                    var key = Console.ReadKey(intercept: true).Key;

                    if (key == ConsoleKey.D)
                    {
                        Console.WriteLine("\n\nFehlgeschlagene Spaces:");
                        Console.WriteLine(new string('-', 90));
                        Console.WriteLine("{0,-25} | {1,-25} | {2}", "Name", "Email", "Fehlermeldung");

                        foreach (var fail in failedSpaces)
                        {
                            Console.WriteLine($"{fail.Name,-25} | {fail.Email,-25} | {fail.Error}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Laden der Hauptliste: {ex.Message}");
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
