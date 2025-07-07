using System.Text.Json;
using Microsoft.Data.Sqlite;
using HackerspaceLogic.Core;


namespace HackerspaceLogic.Core;

public static class Downloadator
{
    private const string ApiUrl = "https://mapall.space/api.json";

    public static async Task DownloadRawAsync()
    {
        string dbPath = DbPathHelper.GetDatabasePath();
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        using var client = new HttpClient();
        string rootJson = await client.GetStringAsync(ApiUrl);
        using var doc = JsonDocument.Parse(rootJson);

        if (!doc.RootElement.TryGetProperty("features", out var features) || features.ValueKind != JsonValueKind.Array)
        {
            Console.WriteLine("❌ Keine Features gefunden.");
            return;
        }

        using var connection = new SqliteConnection($"Data Source={dbPath}");
        await connection.OpenAsync();

        // 🛠️ Tabelle erstellen, falls sie nicht existiert
        var createCmd = connection.CreateCommand();
        createCmd.CommandText = """
            CREATE TABLE IF NOT EXISTS RawHackerspaceData (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                FeatureJson TEXT NOT NULL,
                SourceJson TEXT
            );
        """;
        await createCmd.ExecuteNonQueryAsync();

        // 🧹 Bestehende Daten löschen
        var clearCmd = connection.CreateCommand();
        clearCmd.CommandText = "DELETE FROM RawHackerspaceData;";
        await clearCmd.ExecuteNonQueryAsync();

        // 🔄 AUTOINCREMENT-Reset
        var resetCmd = connection.CreateCommand();
        resetCmd.CommandText = "DELETE FROM sqlite_sequence WHERE name='RawHackerspaceData';";
        await resetCmd.ExecuteNonQueryAsync();

        // 📦 Alle Features einfügen
        foreach (var feature in features.EnumerateArray())
        {
            if (!feature.TryGetProperty("properties", out var props)) continue;

            string name = props.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? "???" : "???";
            string sourceUrl = props.TryGetProperty("source", out var srcProp) ? srcProp.GetString() ?? "" : "";

            string featureJson = feature.GetRawText();
            string? sourceJson = null;

            if (!string.IsNullOrWhiteSpace(sourceUrl))
            {
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                    string raw = await client.GetStringAsync(sourceUrl, cts.Token);
                    using var tmp = JsonDocument.Parse(raw);
                    sourceJson = tmp.RootElement.GetRawText();
                }
                catch
                {
                    // 🌐 Quelle nicht erreichbar → SourceJson bleibt null
                }
            }

            var insert = connection.CreateCommand();
            insert.CommandText = """
                INSERT INTO RawHackerspaceData (Name, FeatureJson, SourceJson)
                VALUES ($name, $feature, $source);
            """;
            insert.Parameters.AddWithValue("$name", name);
            insert.Parameters.AddWithValue("$feature", featureJson);
            insert.Parameters.AddWithValue("$source", (object?)sourceJson ?? DBNull.Value);
            await insert.ExecuteNonQueryAsync();
        }

        Console.WriteLine("✅ Rohdaten gespeichert (inkl. ID-Reset).");
    }

    public static async Task<int> GetFeatureCountAsync()
    {
        using var client = new HttpClient();
        string rootJson = await client.GetStringAsync(ApiUrl);
        using var doc = JsonDocument.Parse(rootJson);
        return doc.RootElement.TryGetProperty("features", out var features) && features.ValueKind == JsonValueKind.Array
            ? features.GetArrayLength()
            : 0;
    }

    public static async IAsyncEnumerable<bool> DownloadRawWithProgressAsync(Action<int, int> progressCallback)
    {
        string dbPath = DbPathHelper.GetDatabasePath();
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        using var client = new HttpClient();
        string rootJson = await client.GetStringAsync(ApiUrl);
        using var doc = JsonDocument.Parse(rootJson);

        if (!doc.RootElement.TryGetProperty("features", out var features) || features.ValueKind != JsonValueKind.Array)
            yield break;

        using var connection = new SqliteConnection($"Data Source={dbPath}");
        await connection.OpenAsync();

        // Tabelle & Clear
        var createCmd = connection.CreateCommand();
        createCmd.CommandText = """
        CREATE TABLE IF NOT EXISTS RawHackerspaceData (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            FeatureJson TEXT NOT NULL,
            SourceJson TEXT
        );
    """;
        await createCmd.ExecuteNonQueryAsync();

        var clearCmd = connection.CreateCommand();
        clearCmd.CommandText = "DELETE FROM RawHackerspaceData;";
        await clearCmd.ExecuteNonQueryAsync();

        var resetCmd = connection.CreateCommand();
        resetCmd.CommandText = "DELETE FROM sqlite_sequence WHERE name='RawHackerspaceData';";
        await resetCmd.ExecuteNonQueryAsync();

        int total = features.GetArrayLength();
        int count = 0;

        foreach (var feature in features.EnumerateArray())
        {
            count++;
            progressCallback?.Invoke(count, total);

            if (!feature.TryGetProperty("properties", out var props)) continue;

            string name = props.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? "???" : "???";
            string sourceUrl = props.TryGetProperty("source", out var srcProp) ? srcProp.GetString() ?? "" : "";

            string featureJson = feature.GetRawText();
            string? sourceJson = null;

            if (!string.IsNullOrWhiteSpace(sourceUrl))
            {
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                    string raw = await client.GetStringAsync(sourceUrl, cts.Token);
                    using var tmp = JsonDocument.Parse(raw);
                    sourceJson = tmp.RootElement.GetRawText();
                }
                catch { }
            }

            var insert = connection.CreateCommand();
            insert.CommandText = """
            INSERT INTO RawHackerspaceData (Name, FeatureJson, SourceJson)
            VALUES ($name, $feature, $source);
        """;
            insert.Parameters.AddWithValue("$name", name);
            insert.Parameters.AddWithValue("$feature", featureJson);
            insert.Parameters.AddWithValue("$source", (object?)sourceJson ?? DBNull.Value);
            await insert.ExecuteNonQueryAsync();

            yield return true; // nur zur Schleife in UI
        }
    }


}
