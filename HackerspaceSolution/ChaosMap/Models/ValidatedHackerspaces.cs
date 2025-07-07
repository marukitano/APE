namespace HackerspaceLogic.Core.Models;

using Microsoft.Maui.Graphics;

public class ValidatedHackerspace
{
    public string Name { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string LogoUrl { get; set; } = "";
    public string Status { get; set; } = "";
    public string Validated { get; set; } = "";

    public string KoordinatenString => $"Lat: {Latitude:F4}, Lon: {Longitude:F4}";
    public string StatusWithTimestamp => Status;

    public Color StatusColor => Status.ToLower() switch
    {
        "open" => Colors.Green,
        "closed" => Colors.Red,
        _ => Colors.Gray
    };

}