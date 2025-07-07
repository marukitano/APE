using System.Net.Http;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace HackerspaceLogic.Core;

public static class Validator
{
    public static async Task ValidateAndStoreAsync()
    {
        string dbPath = DbPathHelper.GetDatabasePath();
        string logoFolder = Path.Combine(Path.GetDirectoryName(dbPath)!, "logos");

        Directory.CreateDirectory(logoFolder); // 🗂 Logo-Verzeichnis sicherstellen

        using var connection = new SqliteConnection($"Data Source={dbPath}");
        await connection.OpenAsync();

        // 🔨 Tabelle anlegen (falls nicht vorhanden)
        var createCmd = connection.CreateCommand();
        createCmd.CommandText =
        @"
            CREATE TABLE IF NOT EXISTS ValidatedHackerspaceData (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Latitude REAL NOT NULL,
                Longitude REAL NOT NULL,
                LogoUrl TEXT,
                LogoLocalPath TEXT,
                Status TEXT NOT NULL,
                Validated TEXT NOT NULL
            );
        ";
        await createCmd.ExecuteNonQueryAsync();

        // 🧼 Alte Daten löschen
        var deleteCmd = connection.CreateCommand();
        deleteCmd.CommandText = "DELETE FROM ValidatedHackerspaceData;";
        await deleteCmd.ExecuteNonQueryAsync();

        var resetIdCmd = connection.CreateCommand();
        resetIdCmd.CommandText = "DELETE FROM sqlite_sequence WHERE name='ValidatedHackerspaceData';";
        await resetIdCmd.ExecuteNonQueryAsync();

        // 📦 Rohdaten holen
        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT Name, FeatureJson, SourceJson FROM RawHackerspaceData;";
        using var reader = await selectCmd.ExecuteReaderAsync();

        using var httpClient = new HttpClient();

        while (await reader.ReadAsync())
        {
            string name = reader.GetString(0);
            string featureJson = reader.GetString(1);
            string? sourceJson = reader.IsDBNull(2) ? null : reader.GetString(2);

            double lat = 0, lon = 0;
            string logoUrl = "", logoLocalPath = "";
            string status = "unbekannt";

            // 🌍 Koordinaten
            try
            {
                using var featureDoc = JsonDocument.Parse(featureJson);
                if (featureDoc.RootElement.TryGetProperty("geometry", out var geo) &&
                    geo.TryGetProperty("coordinates", out var coords) &&
                    coords.ValueKind == JsonValueKind.Array &&
                    coords.GetArrayLength() == 2)
                {
                    lon = coords[0].GetDouble();
                    lat = coords[1].GetDouble();
                }
            }
            catch { continue; }

            if (Math.Abs(lat) < 0.0001 && Math.Abs(lon) < 0.0001)
                continue;

            // 🔍 Source auswerten
            if (!string.IsNullOrWhiteSpace(sourceJson))
            {
                try
                {
                    using var sourceDoc = JsonDocument.Parse(sourceJson);
                    var root = sourceDoc.RootElement;

                    // ✅ Status
                    if (root.TryGetProperty("state", out var state) && state.TryGetProperty("open", out var openProp))
                    {
                        status = openProp.ValueKind switch
                        {
                            JsonValueKind.True => "open",
                            JsonValueKind.False => "closed",
                            _ => "unbekannt"
                        };
                    }

                    // 🖼️ Logo verarbeiten
                    if (root.TryGetProperty("logo", out var logoProp) && logoProp.ValueKind == JsonValueKind.String)
                    {
                        logoUrl = logoProp.GetString() ?? "";

                        if (!string.IsNullOrWhiteSpace(logoUrl))
                        {
                            try
                            {
                                string fileName = Path.GetFileName(new Uri(logoUrl).LocalPath);
                                string localPath = Path.Combine(logoFolder, $"{name}_{fileName}");

                                // Nur downloaden, wenn Datei fehlt
                                if (!File.Exists(localPath))
                                {
                                    var data = await httpClient.GetByteArrayAsync(logoUrl);
                                    await File.WriteAllBytesAsync(localPath, data);
                                }

                                logoLocalPath = localPath;
                            }
                            catch
                            {
                                logoLocalPath = ""; // Fehler beim Download ignorieren
                            }
                        }
                    }
                }
                catch { }
            }

            string validated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // 💾 In Datenbank speichern
            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText =
            @"
                INSERT INTO ValidatedHackerspaceData
                (Name, Latitude, Longitude, LogoUrl, LogoLocalPath, Status, Validated)
                VALUES ($name, $lat, $lon, $logoUrl, $logoLocal, $status, $validated);
            ";
            insertCmd.Parameters.AddWithValue("$name", name);
            insertCmd.Parameters.AddWithValue("$lat", lat);
            insertCmd.Parameters.AddWithValue("$lon", lon);
            insertCmd.Parameters.AddWithValue("$logoUrl", logoUrl);
            insertCmd.Parameters.AddWithValue("$logoLocal", logoLocalPath);
            insertCmd.Parameters.AddWithValue("$status", status);
            insertCmd.Parameters.AddWithValue("$validated", validated);

            await insertCmd.ExecuteNonQueryAsync();
        }

        Console.WriteLine("✅ Validierung abgeschlossen – Logos gespeichert.");
    }
}
