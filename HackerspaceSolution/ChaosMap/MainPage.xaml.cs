namespace ChaosMap;

using HackerspaceLogic.Core;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnDownloadClicked(object sender, EventArgs e)
    {
        DownloadButton.Text = "Lade herunter...";

        int total = await Downloadator.GetFeatureCountAsync();  // neu
        int done = 0;

        await foreach (var _ in Downloadator.DownloadRawWithProgressAsync((index, max) =>
                       {
                           done = index;
                           MainThread.BeginInvokeOnMainThread(() =>
                           {
                               DownloadButton.Text = $"{done}/{max} geladen...";
                           });
                       }))
        {
            // hier kommt nix rein – nur fürs await foreach
        }

        await Validator.ValidateAndStoreAsync();

        DownloadButton.Text = "✅ Fertig!";
    }
}