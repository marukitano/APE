using HackerspaceFinder.ViewModel;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System.Net.NetworkInformation;

namespace HackerspaceFinder;

public partial class MapPage : ContentPage
{
    private readonly MapViewModel _viewModel;

    public MapPage()
    {
        InitializeComponent();
        System.Diagnostics.Trace.WriteLine("MapPage geladen");
    }

    public MapPage(MapViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadHackerspacesAsync();
        AddPinsToMap();
    }

    private void AddPinsToMap()
    {
        HackerspaceMap.Pins.Clear();

        foreach (var space in _viewModel.Hackerspaces)
        {
            if (space.Latitude == null || space.Longitude == null)
                continue;

            var pin = new Pin
            {
                Label = space.Name,
                Address = space.Address,
                Location = new Location(space.Latitude.Value, space.Longitude.Value),
                Type = PinType.Place
            };

            HackerspaceMap.Pins.Add(pin);
        }

        // Karte zentrieren (z.B. auf Zürich)
        HackerspaceMap.MoveToRegion(
            MapSpan.FromCenterAndRadius(
                new Location(47.3769, 8.5417), Distance.FromKilometers(100)));
    }
}
