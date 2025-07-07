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

        Directory.CreateDirectory(logoFolder);

        using var connection = new SqliteConnection($"Data Source={dbPath}");
        await connection.OpenAsync();

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
            Validated TEXT NOT NULL,
            Address TEXT,
            Email TEXT,
            Phone TEXT,
            Url TEXT,
            Zip TEXT,
            City TEXT
        );
    ";
        await createCmd.ExecuteNonQueryAsync();

        var deleteCmd = connection.CreateCommand();
        deleteCmd.CommandText = "DELETE FROM ValidatedHackerspaceData;";
        await deleteCmd.ExecuteNonQueryAsync();

        var resetIdCmd = connection.CreateCommand();
        resetIdCmd.CommandText = "DELETE FROM sqlite_sequence WHERE name='ValidatedHackerspaceData';";
        await resetIdCmd.ExecuteNonQueryAsync();

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
            string logoUrl = "", logoLocalPath = "", status = "unbekannt";
            string address = "", email = "", phone = "", url = "", zip = "", city = "";

            // 🌍 Koordinaten & Metadaten aus Feature
            try
            {
                using var featureDoc = JsonDocument.Parse(featureJson);
                var root = featureDoc.RootElement;

                if (root.TryGetProperty("geometry", out var geo) &&
                    geo.TryGetProperty("coordinates", out var coords) &&
                    coords.ValueKind == JsonValueKind.Array &&
                    coords.GetArrayLength() == 2)
                {
                    lon = coords[0].GetDouble();
                    lat = coords[1].GetDouble();
                }

                if (root.TryGetProperty("properties", out var props))
                {
                    props.TryGetProperty("address", out var addrProp); address = addrProp.GetString() ?? "";
                    props.TryGetProperty("email", out var emailProp); email = emailProp.GetString() ?? "";
                    props.TryGetProperty("phone", out var phoneProp); phone = phoneProp.GetString() ?? "";
                    props.TryGetProperty("url", out var urlProp); url = urlProp.GetString() ?? "";
                    props.TryGetProperty("zip", out var zipProp); zip = zipProp.GetString() ?? "";
                    props.TryGetProperty("city", out var cityProp); city = cityProp.GetString() ?? "";
                }
            }
            catch { continue; }

            if (Math.Abs(lat) < 0.0001 && Math.Abs(lon) < 0.0001)
                continue;

            // 🔍 Status und Logo aus Source
            if (!string.IsNullOrWhiteSpace(sourceJson))
            {
                try
                {
                    using var sourceDoc = JsonDocument.Parse(sourceJson);
                    var root = sourceDoc.RootElement;

                    if (root.TryGetProperty("state", out var state) && state.TryGetProperty("open", out var openProp))
                    {
                        status = openProp.ValueKind switch
                        {
                            JsonValueKind.True => "open",
                            JsonValueKind.False => "closed",
                            _ => "unbekannt"
                        };
                    }

                    if (root.TryGetProperty("logo", out var logoProp) && logoProp.ValueKind == JsonValueKind.String)
                    {
                        logoUrl = logoProp.GetString() ?? "";

                        if (!string.IsNullOrWhiteSpace(logoUrl))
                        {
                            try
                            {
                                string fileName = $"{name}_{Path.GetFileName(new Uri(logoUrl).LocalPath)}";
                                string localPath = Path.Combine(logoFolder, fileName);

                                if (!File.Exists(localPath))
                                {
                                    var data = await httpClient.GetByteArrayAsync(logoUrl);
                                    await File.WriteAllBytesAsync(localPath, data);
                                }

                                logoLocalPath = localPath;
                            }
                            catch
                            {
                                logoLocalPath = "";
                            }
                        }
                    }
                }
                catch { }
            }

            string validated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText =
            @"
            INSERT INTO ValidatedHackerspaceData
            (Name, Latitude, Longitude, LogoUrl, LogoLocalPath, Status, Validated, Address, Email, Phone, Url, Zip, City)
            VALUES ($name, $lat, $lon, $logoUrl, $logoLocal, $status, $validated, $addr, $email, $phone, $url, $zip, $city);
        ";
            insertCmd.Parameters.AddWithValue("$name", name);
            insertCmd.Parameters.AddWithValue("$lat", lat);
            insertCmd.Parameters.AddWithValue("$lon", lon);
            insertCmd.Parameters.AddWithValue("$logoUrl", logoUrl);
            insertCmd.Parameters.AddWithValue("$logoLocal", logoLocalPath);
            insertCmd.Parameters.AddWithValue("$status", status);
            insertCmd.Parameters.AddWithValue("$validated", validated);
            insertCmd.Parameters.AddWithValue("$addr", address);
            insertCmd.Parameters.AddWithValue("$email", email);
            insertCmd.Parameters.AddWithValue("$phone", phone);
            insertCmd.Parameters.AddWithValue("$url", url);
            insertCmd.Parameters.AddWithValue("$zip", zip);
            insertCmd.Parameters.AddWithValue("$city", city);

            await insertCmd.ExecuteNonQueryAsync();
        }

        Console.WriteLine("✅ Validierung komplett – Logos & Metadaten gespeichert.");
    }

}
