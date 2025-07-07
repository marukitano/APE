namespace ChaosMap.Models;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

public class ValidatedHackerspace
{
    public string Name { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public string LogoUrl { get; set; } = "";
    public string LogoLocalPath { get; set; } = "";

    public string Status { get; set; } = "";
    public string Validated { get; set; } = "";

    public string Url { get; set; } = "";
    public string Address { get; set; } = "";
    public string Zip { get; set; } = "";
    public string City { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";

    // Convenience Properties
    public string KoordinatenString => $"Lat: {Latitude:F4}, Lon: {Longitude:F4}";

    public string StatusWithTimestamp => Status;

    public Color StatusColor => Status.ToLower() switch
    {
        "open" => Colors.Green,
        "closed" => Colors.Red,
        _ => Colors.Gray
    };

    public bool IsImageVisible =>
        LogoLocalPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
        LogoLocalPath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
        LogoLocalPath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
        LogoLocalPath.EndsWith(".gif", StringComparison.OrdinalIgnoreCase);

    public ImageSource? LogoImage =>
        File.Exists(LogoLocalPath) ? ImageSource.FromFile(LogoLocalPath) : null;

    public bool HasLogo => LogoImage != null;
}