using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpaceApiTest.Helper
{
    public static class MapPropertyHelper
    {
        public static async Task RunAsync()
        {
            string apiUrl = "https://mapall.space/api.json";
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };

            try
            {
                string json = await client.GetStringAsync(apiUrl);
                using JsonDocument doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("features", out var features) || features.ValueKind != JsonValueKind.Array)
                {
                    Console.WriteLine("Keine 'features'-Liste gefunden.");
                    return;
                }

                Console.WriteLine();
                Console.WriteLine($"{"Name",-50} | {"Koordinaten",-30} | {"Status",-12} | {"Message",-100} | Logo");
                Console.WriteLine(new string('-', 205));

                int includedCount = 0;

                foreach (var feature in features.EnumerateArray())
                {
                    string name = "???";
                    string coords = "?";
                    string status = "unbekannt";
                    string message = "-";
                    string logo = "-";

                    // Name und Source
                    if (feature.TryGetProperty("properties", out var props) && props.ValueKind == JsonValueKind.Object)
                    {
                        if (props.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String)
                            name = nameProp.GetString() ?? "???";

                        if (props.TryGetProperty("source", out var sourceProp) &&
                            sourceProp.ValueKind == JsonValueKind.String &&
                            !string.IsNullOrWhiteSpace(sourceProp.GetString()))
                        {
                            try
                            {
                                string sourceUrl = sourceProp.GetString()!;
                                string sourceJson = await client.GetStringAsync(sourceUrl);
                                using var sourceDoc = JsonDocument.Parse(sourceJson);
                                var root = sourceDoc.RootElement;

                                if (root.TryGetProperty("state", out var stateProp) && stateProp.ValueKind == JsonValueKind.Object)
                                {
                                    if (stateProp.TryGetProperty("open", out var openProp))
                                    {
                                        if (openProp.ValueKind == JsonValueKind.True)
                                            status = "open";
                                        else if (openProp.ValueKind == JsonValueKind.False)
                                            status = "closed";
                                        else
                                            status = "unbekannt";
                                    }

                                    if (stateProp.TryGetProperty("message", out var msgProp) && msgProp.ValueKind == JsonValueKind.String)
                                    {
                                        message = msgProp.GetString() ?? "-";
                                        if (message.Length > 100)
                                            message = message.Substring(0, 97) + "...";
                                    }
                                }

                                if (root.TryGetProperty("logo", out var logoProp) && logoProp.ValueKind == JsonValueKind.String)
                                    logo = "OK";
                            }
                            catch
                            {
                                status = "unbekannt";
                                logo = "-";
                                message = "-";
                            }
                        }
                    }

                    // Koordinaten aus geometry
                    bool validCoords = false;
                    if (feature.TryGetProperty("geometry", out var geo) &&
                        geo.ValueKind == JsonValueKind.Object &&
                        geo.TryGetProperty("coordinates", out var coordProp) &&
                        coordProp.ValueKind == JsonValueKind.Array &&
                        coordProp.GetArrayLength() == 2 &&
                        coordProp[0].ValueKind == JsonValueKind.Number &&
                        coordProp[1].ValueKind == JsonValueKind.Number)
                    {
                        double lon = coordProp[0].GetDouble();
                        double lat = coordProp[1].GetDouble();

                        if (Math.Abs(lat) > 0.0001 || Math.Abs(lon) > 0.0001)
                        {
                            coords = $"{lat:F5}, {lon:F5}";
                            validCoords = true;
                        }
                    }

                    if (validCoords)
                    {
                        Console.WriteLine($"{name,-50} | {coords,-30} | {status,-12} | {message,-100} | {logo}");
                        includedCount++;
                    }
                }

                Console.WriteLine($"\n✅ Eingetragene Spaces mit gültigen Koordinaten: {includedCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Fehler: {ex.Message}");
            }
        }
    }
}
