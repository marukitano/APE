using ChaosMap.Models;
// dein lokales Models-Verzeichnis
using System.Diagnostics;

namespace ChaosMap.Views;

public partial class DetailPage : ContentPage, IQueryAttributable
{
    private ValidatedHackerspace? entry;

    public DetailPage()
    {
        InitializeComponent();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        Console.WriteLine("? ApplyQueryAttributes wurde aufgerufen");

        if (query.TryGetValue("SelectedHackerspace", out var value) && value is ValidatedHackerspace e)
        {
            entry = e;
            Console.WriteLine($"?? Daten empfangen: {entry.Name}");
            UpdateUI();
        }
        else
        {
            Console.WriteLine("? Kein Hackerspace-Eintrag übergeben!");
        }
    }

    private void UpdateUI()
    {
        if (entry == null) return;

        NameLabel.Text = entry.Name;
        StatusLabel.Text = $"Status: {entry.Status}";
        ValidationLabel.Text = $"Validiert: {entry.Validated}";

        AddressLabel.Text = string.IsNullOrWhiteSpace(entry.Address) ? "Keine Adresse vorhanden" : entry.Address;
        EmailLabel.Text = string.IsNullOrWhiteSpace(entry.Email) ? "Keine E-Mail vorhanden" : entry.Email;
        PhoneLabel.Text = string.IsNullOrWhiteSpace(entry.Phone) ? "Keine Telefonnummer" : entry.Phone;
        UrlLabel.Text = string.IsNullOrWhiteSpace(entry.Url) ? "Keine Website" : entry.Url;

        ZipCityLabel.Text = (!string.IsNullOrWhiteSpace(entry.Zip) || !string.IsNullOrWhiteSpace(entry.City))
            ? $"{entry.Zip} {entry.City}".Trim()
            : "Keine PLZ/Stadt vorhanden";

        // Logo anzeigen, wenn Pfad existiert
        if (!string.IsNullOrWhiteSpace(entry.LogoLocalPath) && File.Exists(entry.LogoLocalPath))
        {
            try
            {
                LogoImage.Source = ImageSource.FromFile(entry.LogoLocalPath);
                LogoImage.IsVisible = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"? Logo konnte nicht geladen werden: {ex.Message}");
                LogoImage.IsVisible = false;
            }
        }
        else
        {
            LogoImage.IsVisible = false;
        }
    }
}
