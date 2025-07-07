using HackerspaceLogic.Models;

namespace ChaosMap.Views;

public partial class DetailPage : ContentPage, IQueryAttributable
{
    public DetailPage()
    {
        InitializeComponent();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        Console.WriteLine("? ApplyQueryAttributes wurde aufgerufen");

        if (query.TryGetValue("SelectedHackerspace", out var value) && value is HackerspaceEntry entry)
        {
            Console.WriteLine($"?? Daten empfangen: {entry.Name}, Status: {entry.Status}");

            NameLabel.Text = entry.Name;
            StatusLabel.Text = $"Status: {entry.Status}";
            ValidationLabel.Text = $"Validiert: {entry.Validated}";

            AddressLabel.Text = entry.Address ?? "Keine Adresse vorhanden";
            EmailLabel.Text = entry.Email ?? "Keine E-Mail vorhanden";
            PhoneLabel.Text = entry.Phone ?? "Keine Telefonnummer vorhanden";

            if (!string.IsNullOrWhiteSpace(entry.LogoLocalPath))
            {
                LogoImage.Source = ImageSource.FromFile(entry.LogoLocalPath);
            }
            else
            {
                LogoImage.IsVisible = false;
            }
        }
        else
        {
            Console.WriteLine("? Kein Hackerspace-Eintrag übergeben!");
        }
    }
}