namespace HackerspaceFinder.Model;

public class Hackerspace
{
    public string Name { get; set; }
    public string Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // 👇 Diese Felder fehlen dir laut Fehlermeldung:
    public string WebsiteUrl { get; set; }
    public bool IsOpen { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
}
