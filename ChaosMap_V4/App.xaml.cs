using ChaosMap_V4.Services;
using System.Diagnostics;


namespace ChaosMap_V4;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        _ = Task.Run(async () =>
        {
            try
            {
                var provider = new SpaceApiHttpProvider();
                var json = await provider.LoadJsonAsync();

                Debug.WriteLine("✅ API-Antwort empfangen.");
                Debug.WriteLine(json.Substring(0, 500));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("❌ Fehler beim Abruf: " + ex.Message);
            }
        });
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        return new Window(new ContentPage
        {
            Content = new Label
            {
                Text = "HackerspaceFinder gestartet.",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        });
    }
}
