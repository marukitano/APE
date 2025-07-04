using SpaceApiTest.Helper;

bool running = true;

while (running)
{
    Console.Clear(); // Menü wird jedes Mal neu gezeigt
    Console.WriteLine("SpaceAPI Tools");
    Console.WriteLine("---------------------");
    Console.WriteLine("[1] Property-Felder scannen");
    Console.WriteLine("[2] Akteulle Liste aller Spaces Laden");
    Console.WriteLine("[3] Source-JSON-Felder statistik");
    Console.WriteLine("[4] Mapdaten extrahieren");
    Console.WriteLine("[5] LabInspector");

    Console.WriteLine("[0] Beenden");
    Console.Write("Eingabe: ");

    var input = Console.ReadLine();

    switch (input)
    {
        case "1":
            await DynamicPropertyScanner.RunAsync();
            break;

        case "2":
            await DynamicSpaceList.RunAsync();
            break;

        case "3":
            await RemoteSourceScanner.RunAsync();
            break;

        case "4":
            await MapPropertyHelper.RunAsync();
            break;

        case "5":
            await LabInspector.RunAsync();
            break;

        case "0":
            running = false;
            Console.WriteLine("Tschüss!");
            break;

        default:
            Console.WriteLine("Ungültige Eingabe.");
            break;
    }

    if (running)
    {
        Console.WriteLine();
        Console.WriteLine("Drücke eine Taste, um zum Menü zurückzukehren...");
        Console.ReadKey(true);
    }
}
