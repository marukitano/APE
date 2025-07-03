namespace ChaosMap;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

#if DEBUG
        await TestRunners.API_Testzugriff.RunAsync();
#endif
    }
}
