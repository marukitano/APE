using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using HackerspaceLogic.Core.Models;
using Microsoft.Data.Sqlite;
using ChaosMap.ViewModels;


namespace ChaosMap.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<ValidatedHackerspace> Hackerspaces { get; set; } = new();

    private string _lastUpdatedText = "";
    public string LastUpdatedText
    {
        get => _lastUpdatedText;
        set
        {
            if (_lastUpdatedText != value)
            {
                _lastUpdatedText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FormattedListHeader));
            }
        }
    }

    public string FormattedListHeader =>
        $"Liste des Chaos {(string.IsNullOrEmpty(LastUpdatedText) ? "" : $"({_lastUpdatedText})")}";

    public async Task LoadValidatedDataAsync()
    {
        string dbPath = HackerspaceLogic.Core.DbPathHelper.GetDatabasePath();

        if (!File.Exists(dbPath))
            return;

        using var connection = new SqliteConnection($"Data Source={dbPath}");
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Name, Latitude, Longitude, LogoUrl, Status, Validated FROM ValidatedHackerspaceData";

        using var reader = await command.ExecuteReaderAsync();

        Hackerspaces.Clear();

        while (await reader.ReadAsync())
        {
            Hackerspaces.Add(new ValidatedHackerspace
            {
                Name = reader.GetString(0),
                Latitude = reader.GetDouble(1),
                Longitude = reader.GetDouble(2),
                LogoUrl = reader.IsDBNull(3) ? "" : reader.GetString(3),
                Status = reader.GetString(4),
                Validated = reader.GetString(5)
            });
        }

        OnPropertyChanged(nameof(Hackerspaces));

        if (Hackerspaces.Any())
            LastUpdatedText = $"zuletzt geupdatet am {Hackerspaces.Max(h => h.Validated)}";
        else
            LastUpdatedText = "noch keine Daten";
    }

    public void SortAlphabetically()
    {
        var sorted = Hackerspaces.OrderBy(h => h.Name).ToList();
        Hackerspaces.Clear();
        foreach (var hs in sorted)
            Hackerspaces.Add(hs);
    }

    public void SortByOpenStatus()
    {
        var sorted = Hackerspaces
            .OrderBy(h =>
            {
                return h.Status.ToLower() switch
                {
                    "open" => 0,
                    "closed" => 1,
                    _ => 2 // "unknown" oder sonstiges
                };
            })
            .ThenBy(h => h.Name)
            .ToList();

        Hackerspaces.Clear();
        foreach (var hs in sorted)
            Hackerspaces.Add(hs);
    }


    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
