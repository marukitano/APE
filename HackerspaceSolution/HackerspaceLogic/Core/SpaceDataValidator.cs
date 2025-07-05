using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace HackerspaceLogic.Core
{
    public static class SpaceDataValidator
    {
        public static async Task ValidateAndStoreAsync()
        {
            string dbPath = Path.Combine("Database", "hackerspace.db");

            using var connection = new SqliteConnection($"Data Source={dbPath}");
            await connection.OpenAsync();

            // 🛠️ Tabelle für validierte Daten erstellen (falls sie nicht existiert)
            var createCmd = connection.CreateCommand();
            createCmd.CommandText =
            @"
                CREATE TABLE IF NOT EXISTS ValidatedHackerspaceData (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Latitude REAL NOT NULL,
                    Longitude REAL NOT NULL,
                    LogoUrl TEXT,
                    Status TEXT NOT NULL,
                    Validated TEXT NOT NULL
                );
            ";
            await createCmd.ExecuteNonQueryAsync();

            // 🧹 Bestehende Daten leeren
            var deleteCmd = connection.CreateCommand();
            deleteCmd.CommandText = "DELETE FROM ValidatedHackerspaceData;";
            await deleteCmd.ExecuteNonQueryAsync();

            // 🔄 AUTOINCREMENT zurücksetzen (Id wieder bei 1 starten)
            var resetIdCmd = connection.CreateCommand();
            resetIdCmd.CommandText = "DELETE FROM sqlite_sequence WHERE name='ValidatedHackerspaceData';";
            await resetIdCmd.ExecuteNonQueryAsync();

            // 📦 Rohdaten laden
            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT Name, FeatureJson, SourceJson FROM RawHackerspaceData;";
            using var reader = await selectCmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                string name = reader.GetString(0);
                string featureJson = reader.GetString(1);
                string? sourceJson = reader.IsDBNull(2) ? null : reader.GetString(2);

                double lat = 0, lon = 0;
                string logo = "";
                string status = "unbekannt";

                // 🛰️ Koordinaten aus Feature-JSON holen
                try
                {
                    using var featureDoc = JsonDocument.Parse(featureJson);
                    if (featureDoc.RootElement.TryGetProperty("geometry", out var geo) &&
                        geo.ValueKind == JsonValueKind.Object &&
                        geo.TryGetProperty("coordinates", out var coords) &&
                        coords.ValueKind == JsonValueKind.Array &&
                        coords.GetArrayLength() == 2)
                    {
                        lon = coords[0].GetDouble();
                        lat = coords[1].GetDouble();
                    }
                }
                catch
                {
                    continue; // ❌ Fehlerhafte Koordinaten → skip
                }

                if (Math.Abs(lat) < 0.0001 && Math.Abs(lon) < 0.0001)
                    continue; // 🛑 0-Koordinaten → skip

                // 🌐 Auswertung der Source-JSON
                if (!string.IsNullOrWhiteSpace(sourceJson))
                {
                    try
                    {
                        using var sourceDoc = JsonDocument.Parse(sourceJson);
                        var root = sourceDoc.RootElement;

                        if (root.TryGetProperty("state", out var state) && state.ValueKind == JsonValueKind.Object)
                        {
                            if (state.TryGetProperty("open", out var openProp))
                            {
                                status = openProp.ValueKind switch
                                {
                                    JsonValueKind.True => "open",
                                    JsonValueKind.False => "closed",
                                    _ => "unbekannt"
                                };
                            }
                        }

                        if (root.TryGetProperty("logo", out var logoProp) &&
                            logoProp.ValueKind == JsonValueKind.String)
                        {
                            logo = logoProp.GetString() ?? "";
                        }
                    }
                    catch
                    {
                        status = "unbekannt";
                        // logo darf leer bleiben
                    }
                }

                string validated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // 💾 In Datenbank einfügen
                var insertCmd = connection.CreateCommand();
                insertCmd.CommandText =
                @"
                    INSERT INTO ValidatedHackerspaceData (Name, Latitude, Longitude, LogoUrl, Status, Validated)
                    VALUES ($name, $lat, $lon, $logo, $status, $validated);
                ";
                insertCmd.Parameters.AddWithValue("$name", name);
                insertCmd.Parameters.AddWithValue("$lat", lat);
                insertCmd.Parameters.AddWithValue("$lon", lon);
                insertCmd.Parameters.AddWithValue("$logo", logo);
                insertCmd.Parameters.AddWithValue("$status", status);
                insertCmd.Parameters.AddWithValue("$validated", validated);

                await insertCmd.ExecuteNonQueryAsync();
            }

            Console.WriteLine("✅ Validierte Daten gespeichert (inkl. ID-Reset).");
        }
    }
}
