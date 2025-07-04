using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SpaceApiTest.Helper
{
    public static class StatusHelper
    {
        public static async Task RunAsync()
        {
            string apiUrl = "https://mapall.space/api.json";
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(2);

            try
            {
                string rootJson = await client.GetStringAsync(apiUrl);
                using JsonDocument doc = JsonDocument.Parse(rootJson);

                if (!doc.RootElement.TryGetProperty("features", out var features) || features.ValueKind != JsonValueKind.Array)
                {
                    Console.WriteLine("Keine 'features'-Liste gefunden.");
                    return;
                }

                var failedSpaces = new List<string>();
                int total = features.GetArrayLength();
                int success = 0;

                Console.WriteLine();
                Console.WriteLine($"{"Name",-40} | {"Status",-8}");
                Console.WriteLine(new string('-', 55));

                int counter = 0;

                foreach (var feature in features.EnumerateArray())
                {
                    counter++;

                    if (!feature.TryGetProperty("properties", out var props)) continue;

                    string name = props.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? "???" : "???";
                    string sourceUrl = props.TryGetProperty("source", out var srcProp) ? srcProp.GetString() ?? "" : "";

                    string status = "?";

                    if (!string.IsNullOrWhiteSpace(sourceUrl))
                    {
                        try
                        {
                            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                            string json = await client.GetStringAsync(sourceUrl, cts.Token);
                            using var sourceDoc = JsonDocument.Parse(json);
                            var root = sourceDoc.RootElement;

                            if (root.TryGetProperty("state", out var stateProp) &&
                                stateProp.ValueKind == JsonValueKind.Object &&
                                stateProp.TryGetProperty("open", out var openProp))
                            {
                                if (openProp.ValueKind == JsonValueKind.True)
                                    status = "open";
                                else if (openProp.ValueKind == JsonValueKind.False)
                                    status = "closed";
                            }

                            success++;
                        }
                        catch
                        {
                            status = "-";
                            failedSpaces.Add(name);
                        }
                    }
                    else
                    {
                        status = "-";
                        failedSpaces.Add(name);
                    }

                    Console.WriteLine($"{name,-40} | {status,-8}");
                }

                Console.WriteLine();
                Console.WriteLine($"✅ Erfolgreich geladen: {success} / {total}");
                Console.WriteLine($"❌ Fehlerhafte Quellen: {failedSpaces.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Fehler beim Laden der Hauptliste: {ex.Message}");
            }
        }
    }
}
