using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HackerspaceLogic.Helper
{
    public static class StatusHelper
    {
        public static async Task RunAsync()
        {
            string apiUrl = "https://mapall.space/api.json";
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };

            try
            {
                string rootJson = await client.GetStringAsync(apiUrl);
                using JsonDocument doc = JsonDocument.Parse(rootJson);

                if (!doc.RootElement.TryGetProperty("features", out var features) || features.ValueKind != JsonValueKind.Array)
                {
                    Console.WriteLine("❌ Keine 'features'-Liste gefunden.");
                    return;
                }

                int openCount = 0;
                var failedSpaces = new List<string>();

                Console.WriteLine();
                Console.WriteLine($"{"🟢 Offene Spaces:",-50}");
                Console.WriteLine(new string('-', 60));

                foreach (var feature in features.EnumerateArray())
                {
                    if (!feature.TryGetProperty("properties", out var props)) continue;

                    string name = props.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? "???" : "???";
                    string sourceUrl = props.TryGetProperty("source", out var srcProp) ? srcProp.GetString() ?? "" : "";

                    if (string.IsNullOrWhiteSpace(sourceUrl))
                    {
                        failedSpaces.Add(name);
                        continue;
                    }

                    try
                    {
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
                        string json = await client.GetStringAsync(sourceUrl, cts.Token);
                        using var sourceDoc = JsonDocument.Parse(json);
                        var root = sourceDoc.RootElement;

                        if (root.TryGetProperty("state", out var stateProp) &&
                            stateProp.ValueKind == JsonValueKind.Object &&
                            stateProp.TryGetProperty("open", out var openProp) &&
                            openProp.ValueKind == JsonValueKind.True)
                        {
                            Console.WriteLine($"- {name}");
                            openCount++;
                        }
                    }
                    catch
                    {
                        failedSpaces.Add(name);
                    }
                }

                Console.WriteLine();
                Console.WriteLine($"✅ Offen: {openCount}");
                Console.WriteLine($"⚠️  Fehlerhafte Quellen: {failedSpaces.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Fehler beim Laden der Hauptliste: {ex.Message}");
            }
        }
    }
}
