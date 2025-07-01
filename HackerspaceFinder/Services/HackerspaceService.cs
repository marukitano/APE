using System.Net.Http.Json;
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
        const string apiUrl = "https://spaceapi.io/stuff/mapall-space/";

        try
        {
            var rawList = await _httpClient.GetFromJsonAsync<List<RawHackerspace>>(apiUrl);
            if (rawList == null) return new();

            return rawList
                .Where(x => x.location?.lat != null && x.location?.lon != null)
                .Select(x => new Hackerspace
                {
                    Name = x.name,
                    Address = x.location?.address ?? "Unbekannt",
                    Latitude = x.location!.lat!.Value,
                    Longitude = x.location.lon!.Value,
                    WebsiteUrl = x.contact?.web ?? "",
                    IsOpen = x.state?.open ?? false,
                    Phone = x.contact?.phone ?? "",
                    Email = x.contact?.email ?? ""
                }).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden der API: {ex.Message}");
            return new();
        }
    }

    // Diese Klasse entspricht der Struktur der JSON-API
    private class RawHackerspace
    {
        public string name { get; set; }
        public Location? location { get; set; }
        public Contact? contact { get; set; }
        public State? state { get; set; }

        public class Location
        {
            public double? lat { get; set; }
            public double? lon { get; set; }
            public string? address { get; set; }
        }

        public class Contact
        {
            public string? web { get; set; }
            public string? phone { get; set; }
            public string? email { get; set; }
        }

        public class State
        {
            public bool? open { get; set; }
        }
    }
}
