﻿using Microsoft.Extensions.Logging;
using MLZ2025.ViewModel;

namespace MLZ2025;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services
            .AddSingleton<MainPage>()
            .AddSingleton<MainViewModel>()
            .AddTransient<DetailPage>()
            .AddTransient<DetailViewModel>()
            .AddSingleton(Connectivity.Current);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
