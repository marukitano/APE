using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace HackerspaceLogic.Helper;

public static class InteractiveSpaceExplorer
{
    public static async Task RunAsync()
    {
        try
        {
            var featuresUrl = "https://mapall.space/api.json";
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

            var featureJson = await httpClient.GetStringAsync(featuresUrl);
            using var featureDoc = JsonDocument.Parse(featureJson);
            var features = featureDoc.RootElement.GetProperty("features").EnumerateArray().ToList();

            var nameMap = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);

            foreach (var feature in features)
            {
                if (feature.TryGetProperty("properties", out var props) &&
                    props.TryGetProperty("name", out var nameProp) &&
                    nameProp.ValueKind == JsonValueKind.String)
                {
                    string name = nameProp.GetString() ?? "???";
                    nameMap[name] = feature;
                }
            }

            var names = nameMap.Keys.OrderBy(n => n).ToList();

            while (true)
            {
                Console.WriteLine("\n🔍 Verfügbare Hackerspaces:");
                for (int i = 0; i < names.Count; i++)
                    Console.WriteLine($"[{i + 1}] {names[i]}");

                Console.Write("\nWähle eine Nummer (oder 'E' für Exit): ");
                var input = Console.ReadLine()?.Trim();

                if (string.Equals(input, "e", StringComparison.OrdinalIgnoreCase))
                    break;

                if (!int.TryParse(input, out int index) || index < 1 || index > names.Count)
                {
                    Console.WriteLine("❌ Ungültige Eingabe.");
                    continue;
                }

                var selectedName = names[index - 1];
                var selectedFeature = nameMap[selectedName];

                Console.WriteLine($"\n🧩 Feature aus mapall.space für: {selectedName}");
                Console.WriteLine(new string('-', 60));
                DumpTreeElement(selectedFeature);

                if (!selectedFeature.TryGetProperty("properties", out var propsElement) ||
                    !propsElement.TryGetProperty("source", out var sourceUrlProp) ||
                    sourceUrlProp.ValueKind != JsonValueKind.String)
                {
                    Console.WriteLine("⚠️ Kein gültiger source-URL im Feature gefunden.");
                    ContinuePrompt();
                    continue;
                }

                var sourceUrl = sourceUrlProp.GetString() ?? "";
                if (string.IsNullOrWhiteSpace(sourceUrl))
                {
                    Console.WriteLine("⚠️ Leere source-URL.");
                    ContinuePrompt();
                    continue;
                }

                Console.WriteLine($"\n🌐 Lade source.json von: {sourceUrl}");

                try
                {
                    var detailJson = await httpClient.GetStringAsync(sourceUrl);
                    using var detailDoc = JsonDocument.Parse(detailJson);

                    Console.WriteLine($"\n📦 Inhalt von source.json für: {selectedName}");
                    Console.WriteLine(new string('-', 60));
                    DumpTreeElement(detailDoc.RootElement);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Fehler beim Laden der source.json: {ex.Message}");
                }

                ContinuePrompt();
            }

            Console.WriteLine("\n👋 Tschüss – bis zur nächsten Inspektion!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Fehler: {ex.Message}");
        }
    }

    private static void ContinuePrompt()
    {
        Console.WriteLine("\n[Enter] für neue Auswahl, [E]xit zum Beenden");
        var key = Console.ReadKey(intercept: true).KeyChar;
        if (char.ToLower(key) == 'e')
        {
            Console.WriteLine("\n👋 Tschüss!");
            Environment.Exit(0);
        }

        Console.WriteLine(); // Leerzeile
    }


    private static void DumpTreeElement(JsonElement element, string indent = "", bool isLast = true, int depth = 0)
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
                    DumpTreeElement(prop.Value, newIndent, last, depth + 1);
                }

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
                Console.WriteLine($"{indent}{prefix}[{type}]{(string.IsNullOrEmpty(preview) ? "" : $" : {preview}")}");

                if (item.ValueKind == JsonValueKind.Object || item.ValueKind == JsonValueKind.Array)
                {
                    var newIndent = indent + (last ? "    " : "│   ");
                    DumpTreeElement(item, newIndent, last, depth + 1);
                }
            }
        }
    }

    private static string ValuePreview(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? "",
            JsonValueKind.Number => element.TryGetDouble(out var d) ? d.ToString("G17") : element.ToString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => "null",
            JsonValueKind.Array => TryPreviewArray(element),
            JsonValueKind.Object => "",
            _ => element.ToString()
        };
    }

    private static string TryPreviewArray(JsonElement arrayElement)
    {
        try
        {
            var values = arrayElement.EnumerateArray()
                .Take(5)
                .Select(v =>
                {
                    if (v.ValueKind == JsonValueKind.Number && v.TryGetDouble(out var d))
                        return d.ToString("G17");

                    if (v.ValueKind == JsonValueKind.String)
                        return $"\"{v.GetString()}\"";

                    return v.ToString();
                });

            return "[" + string.Join(", ", values) +
                   (arrayElement.GetArrayLength() > 5 ? ", …" : "") + "]";
        }
        catch
        {
            return $"Array({arrayElement.GetArrayLength()})";
        }
    }
}
