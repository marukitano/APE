using System.Text.Json;

namespace HackerspaceLogic.Helper
{
    public class SpaceInfo
    {
        public string? Name { get; set; }
        public string? SourceUrl { get; set; }
    }

    public class DynamicSpaceList
    {
        public static async Task<List<SpaceInfo>> LoadAllNamesAsync()
        {
            var url = "https://mapall.space/api.json";
            using var httpClient = new HttpClient();
            var jsonString = await httpClient.GetStringAsync(url);

            var jsonDoc = JsonDocument.Parse(jsonString);
            var root = jsonDoc.RootElement;

            var list = new List<SpaceInfo>();

            if (!root.TryGetProperty("features", out var features) || features.ValueKind != JsonValueKind.Array)
                return list;

            foreach (var feature in features.EnumerateArray())
            {
                if (!feature.TryGetProperty("properties", out var props))
                    continue;

                string name = props.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? "(kein Name)" : "(kein Name)";
                string sourceUrl = "";

                if (props.TryGetProperty("source", out var sourceProp) && sourceProp.ValueKind == JsonValueKind.String)
                    sourceUrl = sourceProp.GetString() ?? "";
                else if (props.TryGetProperty("url", out var urlProp) && urlProp.ValueKind == JsonValueKind.String)
                    sourceUrl = urlProp.GetString() ?? "";

                list.Add(new SpaceInfo
                {
                    Name = name,
                    SourceUrl = sourceUrl
                });
            }

            return list;
        }

        public static async Task RunAsync()
        {
            try
            {
                var list = await LoadAllNamesAsync();
                Console.WriteLine($"🔍 Es wurden {list.Count} Hackerspaces gefunden:\n");

                foreach (var space in list)
                    Console.WriteLine($"- {space.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Fehler beim Laden oder Verarbeiten: " + ex.Message);
            }
        }
    }
}
