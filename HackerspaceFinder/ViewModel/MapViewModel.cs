using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using HackerspaceFinder.Model;
using HackerspaceFinder.Services;


namespace HackerspaceFinder.ViewModel;

public class MapViewModel : INotifyPropertyChanged
{
    private readonly IHackerspaceService _hackerspaceService;

    public ObservableCollection<Hackerspace> Hackerspaces { get; } = new();

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy != value)
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }
    }

    public MapViewModel(IHackerspaceService hackerspaceService)
    {
        _hackerspaceService = hackerspaceService;
    }

    public async Task LoadHackerspacesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Hackerspaces.Clear();

            var spaces = await _hackerspaceService.LoadHackerspacesAsync();

            foreach (var space in spaces)
            {
                Hackerspaces.Add(space);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
