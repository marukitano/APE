using System.Net.Http.Json;
using System.Text.Json;
using HackerspaceFinder.Model;

namespace HackerspaceFinder.Services;

public class HackerspaceService : IHackerspaceService
{
    private readonly HttpClient _httpClient;

    public HackerspaceService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Hackerspace>> LoadHackerspacesAsync()
    {
        const string apiUrl = "https://mapall.space/api.json";

        try
        {
            using var stream = await _httpClient.GetStreamAsync(apiUrl);
            using var doc = await JsonDocument.ParseAsync(stream);

            var features = doc.RootElement.GetProperty("features");
            var spaces = new List<Hackerspace>();

            foreach (var feature in features.EnumerateArray())
            {
                if (!feature.TryGetProperty("geometry", out var geometry) ||
                    !geometry.TryGetProperty("coordinates", out var coords) ||
                    coords.GetArrayLength() < 2)
                    continue;

                var latitude = coords[1].GetDouble();
                var longitude = coords[0].GetDouble();

                var props = feature.GetProperty("properties");

                var space = new Hackerspace
                {
                    Name = props.GetProperty("name").GetString() ?? "Unbekannt",
                    Address = string.Join(" ",
                        new[]
                        {
                            props.GetProperty("address").GetString(),
                            props.GetProperty("zip").GetString(),
                            props.GetProperty("city").GetString()
                        }.Where(s => !string.IsNullOrWhiteSpace(s))),
                    Latitude = latitude,
                    Longitude = longitude,
                    WebsiteUrl = props.GetProperty("url").GetString() ?? string.Empty,
                    Email = props.GetProperty("email").GetString() ?? string.Empty,
                    Phone = props.GetProperty("phone").ToString(),
                    IsOpen = false
                };

                spaces.Add(space);
            }

            return spaces;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden der API: {ex.Message}");
            return new();
        }
    }

}
