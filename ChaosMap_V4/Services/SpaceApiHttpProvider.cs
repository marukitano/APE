using System.Net.Http;
using System.Text.Json;
using ChaosMap_V4.Models;

namespace ChaosMap_V4.Services;

public class SpaceApiHttpProvider
{
    private readonly HttpClient _httpClient;

    public SpaceApiHttpProvider(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<string> LoadJsonAsync()
    {
        const string url = "https://mapall.space/api.json";
        return await _httpClient.GetStringAsync(url);
    }

    public async Task<FeatureCollection> LoadFeaturesAsync()
    {
        var json = await LoadJsonAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var featureCollection = JsonSerializer.Deserialize<FeatureCollection>(json, options);
        return featureCollection;
    }
}
