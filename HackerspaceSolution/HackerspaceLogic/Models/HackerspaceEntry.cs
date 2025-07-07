namespace HackerspaceLogic.Models;

public class HackerspaceEntry
{
    public string Name { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public string LogoUrl { get; set; } = "";
    public string LogoLocalPath { get; set; } = "";

    public string Status { get; set; } = "unbekannt";
    public string Validated { get; set; } = "";

    public string Url { get; set; } = "";
    public string Address { get; set; } = "";
    public string Zip { get; set; } = "";
    public string City { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
}