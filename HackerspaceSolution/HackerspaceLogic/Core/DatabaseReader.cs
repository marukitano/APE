using HackerspaceLogic.Models;
using Microsoft.Data.Sqlite;

namespace HackerspaceLogic.Core;

public static class DatabaseReader
{
    public static async Task<List<HackerspaceEntry>> LoadValidatedDataAsync()
    {
        var list = new List<HackerspaceEntry>();
        string dbPath = Path.Combine("Database", "hackerspace.db");

        using var connection = new SqliteConnection($"Data Source={dbPath}");
        await connection.OpenAsync();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Name, Latitude, Longitude, LogoUrl, Status, Validated FROM ValidatedHackerspaceData";

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new HackerspaceEntry
            {
                Name = reader.GetString(0),
                Latitude = reader.GetDouble(1),
                Longitude = reader.GetDouble(2),
                LogoUrl = reader.IsDBNull(3) ? "" : reader.GetString(3),
                Status = reader.GetString(4),
                Validated = reader.GetString(5)
            });
        }

        return list;
    }
}