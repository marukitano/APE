using System.Collections.ObjectModel;
using ChaosMap_V4.Models;
using ChaosMap_V4.Services;

namespace ChaosMap_V4;

public partial class MainPage : ContentPage
{
    // ObservableCollection informiert UI automatisch über Änderungen
    public ObservableCollection<Feature> Features { get; } = new();

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;  // wichtig für DataBinding

        LoadData();
    }

    private async void LoadData()
    {
        var provider = new SpaceApiHttpProvider();

        try
        {
            var data = await provider.LoadFeaturesAsync();
            Features.Clear();
            foreach (var feature in data.features)
            {
                Features.Add(feature);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", $"Daten konnten nicht geladen werden:\n{ex.Message}", "OK");
        }
    }
}
