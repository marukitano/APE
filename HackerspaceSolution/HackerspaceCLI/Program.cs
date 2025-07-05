using HackerspaceLogic.Helper;

namespace HackerspaceCLI;

class Program
{
    static async Task Main()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("🔧 Hackerspace CLI Toolset");
            Console.WriteLine(new string('-', 40));
            Console.WriteLine("1. CLI-SpaceExplorer (zeigt alle Spaces und alle Details)");
            Console.WriteLine("2. Zeige alle Daten von einem Hackerspace (Name des Spaces musst du wissen)");
            Console.WriteLine("3. Zeige alle geoeffneten Spaces");
            Console.WriteLine("4. SourceJSON Datentypen und Statistik (fuers Debugging)");
            Console.WriteLine("5. AllSpacesJSON Datentypen und Statistik (Fuers Debuggen)");
            Console.WriteLine("6. Zeigt eine aktuelle Liste ALLER Spaces aus der SpaceAPI");
            Console.WriteLine("7. Zeige alle Daten, welche in der Map zu sehen sind");
            Console.WriteLine("E. Exit");
            Console.WriteLine(new string('-', 40));
            Console.Write("Wähle ein Tool (1-7 oder E): ");

            var key = Console.ReadKey(intercept: true).KeyChar;
            Console.WriteLine();

            switch (char.ToLower(key))
            {
                case '1':
                    await InteractiveSpaceExplorer.RunAsync();
                    break;
                case '2':
                    await LabInspector.RunAsync();
                    break;
                case '3':
                    await StatusHelper.RunAsync();
                    break;
                case '4':
                    await RemoteSourceScanner.RunAsync();
                    break;
                case '5':
                    await DynamicPropertyScanner.RunAsync();
                    break;
                case '6':
                    await DynamicSpaceList.RunAsync();
                    break;
                case '7':
                    await MapPropertyHelper.RunAsync();
                    break;
                case 'e':
                    Console.WriteLine("👋 Tschüss – bis zum nächsten Hack!");
                    return;
                default:
                    Console.WriteLine("❌ Ungültige Eingabe!");
                    break;
            }

            Console.WriteLine("\n[Enter] für neue Auswahl...");
            Console.ReadLine();
        }
    }
}
