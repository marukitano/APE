using HackerspaceLogic.Helper; // <– Namespace deiner Helper-Klassen

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Starte Hackerspace CLI…");

        // Beispiel: nutze MapPropertyHelper aus deiner Logic
        await MapPropertyHelper.RunAsync();

        Console.WriteLine("Fertig!");
    }
}