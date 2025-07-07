using Microsoft.Maui.Controls;
using System;
using HackerspaceLogic.Core; // wichtig!
using System.Threading.Tasks;

namespace ChaosMap;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnCounterClicked(object sender, EventArgs e)
    {
        CounterBtn.Text = "⬇️ Download läuft...";
        try
        {
            await Downloadator.DownloadRawAsync();
            await Validator.ValidateAndStoreAsync();

            await DisplayAlert("Fertig!", "Download und Validierung abgeschlossen!", "OK");
            CounterBtn.Text = "✅ Erfolgreich!";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
            CounterBtn.Text = "❌ Fehler beim Download";
        }
    }
}