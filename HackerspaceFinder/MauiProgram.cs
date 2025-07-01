using CommunityToolkit.Maui; // für das Community Toolkit
using HackerspaceFinder.Services; // deine Services
using Microsoft.Maui.Controls.Maps; // für Karten
using HackerspaceFinder;
using HackerspaceFinder.ViewModel;



public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit() // 🧰 Community Toolkit aktivieren
            .UseMauiMaps()             // 🗺️ Karten-Funktion aktivieren
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // 💉 Dependency Injection: Services registrieren
        builder.Services.AddSingleton<IHackerspaceService, HackerspaceService>();
        builder.Services.AddSingleton<HttpClient>();

        // 🧠 ViewModel + Page registrieren
        builder.Services.AddSingleton<MapViewModel>();
        builder.Services.AddSingleton<MapPage>();

        return builder.Build();
    }
}
