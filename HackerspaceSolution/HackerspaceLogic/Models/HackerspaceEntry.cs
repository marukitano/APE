namespace HackerspaceLogic.Models;

public class HackerspaceEntry
{
    public string Name { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string LogoUrl { get; set; } = "";
    public string Status { get; set; } = "unbekannt";
    public string Validated { get; set; } = "";
}