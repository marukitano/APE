namespace HackerspaceApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // WICHTIG: AppShell korrekt referenzieren
        MainPage = new AppShell();
    }
}