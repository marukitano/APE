namespace ChaosMap;

using HackerspaceLogic.Core;
using ChaosMap.ViewModels; // Wichtig für das ViewModel

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        // 🧠 Hier wird das ViewModel als Datenkontext gesetzt:
        BindingContext = new MainViewModel();
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

        // ⏩ Liste neu laden:
        if (BindingContext is MainViewModel vm)
            await vm.LoadValidatedDataAsync();

        DownloadButton.Text = "✅ Fertig!";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is MainViewModel vm)
            await vm.LoadValidatedDataAsync();
    }

    private void OnSortAlphaClicked(object sender, EventArgs e)
    {
        if (BindingContext is MainViewModel vm)
            vm.SortAlphabetically();
    }

    private void OnSortStatusClicked(object sender, EventArgs e)
    {
        if (BindingContext is MainViewModel vm)
            vm.SortByOpenStatus();
    }


}