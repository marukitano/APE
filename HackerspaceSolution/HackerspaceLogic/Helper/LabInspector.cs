using System.Text.Json;

namespace HackerspaceLogic.Helper
{
    public static class LabInspector
    {
        public static async Task RunAsync()
        {
            string apiUrl = "https://mapall.space/api.json";
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

            try
            {
                string json = await client.GetStringAsync(apiUrl);
                using var doc = JsonDocument.Parse(json);

                var nameMap = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);

                foreach (var feature in doc.RootElement.GetProperty("features").EnumerateArray())
                {
                    if (feature.TryGetProperty("properties", out var props) &&
                        props.TryGetProperty("name", out var nameProp) &&
                        nameProp.ValueKind == JsonValueKind.String)
                    {
                        string name = nameProp.GetString() ?? "???";
                        nameMap[name] = feature;
                    }
                }

                Console.Write("🔍 Name oder Teilname des Labs: ");
                string input = Console.ReadLine()?.Trim() ?? "";

                var matching = nameMap.Keys
                    .Where(n => n.Contains(input, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(n => n)
                    .ToList();

                if (matching.Count == 0)
                {
                    Console.WriteLine("❌ Kein Lab gefunden.");
                    return;
                }

                Console.WriteLine("\nGefundene Labs:");
                for (int i = 0; i < matching.Count; i++)
                    Console.WriteLine($"[{i + 1}] {matching[i]}");

                Console.Write("Wähle eine Nummer: ");
                if (!int.TryParse(Console.ReadLine(), out int index) || index < 1 || index > matching.Count)
                {
                    Console.WriteLine("❌ Ungültige Auswahl.");
                    return;
                }

                string selectedName = matching[index - 1];
                JsonElement selectedFeature = nameMap[selectedName];

                Console.WriteLine($"\n🔎 Vollständige Daten zu: {selectedName}");
                Console.WriteLine(new string('-', 80));

                DumpElement("features", selectedFeature);

                if (selectedFeature.TryGetProperty("properties", out var propsElement))
                {
                    Console.WriteLine("\n📦 Properties:");
                    DumpTreeElement(propsElement);
                }

                if (selectedFeature.TryGetProperty("geometry", out var geoElement))
                {
                    Console.WriteLine("\n🗺 Geometry:");
                    DumpTreeElement(geoElement);
                }

                // Optional: source.json laden
                if (propsElement.TryGetProperty("source", out var sourceProp) &&
                    sourceProp.ValueKind == JsonValueKind.String)
                {
                    string sourceUrl = sourceProp.GetString() ?? "";
                    if (!string.IsNullOrWhiteSpace(sourceUrl))
                    {
                        Console.WriteLine("\n🌐 Lade verlinkte source.json...");
                        try
                        {
                            string detailJson = await client.GetStringAsync(sourceUrl);
                            using var detailDoc = JsonDocument.Parse(detailJson);

                            Console.WriteLine("\n📄 source.json Inhalt:");
                            DumpTreeElement(detailDoc.RootElement);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Fehler beim Laden der source.json: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Fehler beim Abruf: {ex.Message}");
            }
        }

        private static void DumpElement(string label, JsonElement element)
        {
            Console.WriteLine($"{label,-30} [{element.ValueKind}]");
        }

        private static void DumpTreeElement(JsonElement element, string indent = "", int depth = 0)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                var props = element.EnumerateObject().ToList();

                for (int i = 0; i < props.Count; i++)
                {
                    var prop = props[i];
                    bool last = i == props.Count - 1;
                    var prefix = last ? "└── " : "├── ";

                    string type = prop.Value.ValueKind.ToString();
                    string preview = ValuePreview(prop.Value);
                    string line = $"{indent}{prefix}{prop.Name} [{type}]";

                    if (prop.Value.ValueKind != JsonValueKind.Object &&
                        prop.Value.ValueKind != JsonValueKind.Array)
                    {
                        line += $" : {preview}";
                    }

                    Console.WriteLine(line);

                    if (prop.Value.ValueKind == JsonValueKind.Object || prop.Value.ValueKind == JsonValueKind.Array)
                    {
                        var newIndent = indent + (last ? "    " : "│   ");
                        DumpTreeElement(prop.Value, newIndent, depth + 1);
                    }

                    // Leerzeile bei Top-Level-Props für bessere Lesbarkeit
                    if (depth == 0 && i < props.Count - 1)
                        Console.WriteLine(indent + "│");
                    else if (depth == 0)
                        Console.WriteLine();
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                var items = element.EnumerateArray().ToList();
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    bool last = i == items.Count - 1;
                    var prefix = last ? "└── " : "├── ";
                    string type = item.ValueKind.ToString();
                    string preview = ValuePreview(item);
                    Console.WriteLine($"{indent}{prefix}[{type}] : {preview}");

                    if (item.ValueKind == JsonValueKind.Object || item.ValueKind == JsonValueKind.Array)
                    {
                        var newIndent = indent + (last ? "    " : "│   ");
                        DumpTreeElement(item, newIndent, depth + 1);
                    }
                }
            }
        }

        private static string ValuePreview(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString() ?? "",
                JsonValueKind.Number => element.ToString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Array => $"Array({element.GetArrayLength()})",
                JsonValueKind.Object => "", // nichts anzeigen, weil Baumstruktur darunter folgt
                JsonValueKind.Null => "null",
                _ => element.ToString()
            };
        }
    }
}
