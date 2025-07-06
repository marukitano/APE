using System.Collections.ObjectModel;
using System.Windows.Input;
using HackerspaceLogic.Core;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using HackerspaceLogic.Models;

namespace HackerspaceApp.ViewModels;

public class MapViewModel : BindableObject
{
    public ObservableCollection<HackerspaceEntry> Spaces { get; } = new();
    public ICommand RefreshCommand { get; }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    public MapViewModel()
    {
        RefreshCommand = new Command(async () => await LoadDataAsync());
        Task.Run(LoadDataAsync); // initialer Start
    }

    public async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            await SpaceDataDownloader.DownloadRawAsync();
            await SpaceDataValidator.ValidateAndStoreAsync();

            var entries = await DatabaseReader.LoadValidatedDataAsync();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Spaces.Clear();
                foreach (var entry in entries)
                    Spaces.Add(entry);
            });
        }
        finally
        {
            IsBusy = false;
        }
    }
}