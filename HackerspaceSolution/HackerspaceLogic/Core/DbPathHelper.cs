using System;
using System.IO;

namespace HackerspaceLogic.Core;

public static class DbPathHelper
{
    public static string GetDatabasePath()
    {
        // Holt den aktuellen Arbeitsordner der Anwendung (z. B. bin/Debug/netX.Y/)
        string basePath = AppContext.BaseDirectory;

        // Geht von dort relativ zu einem Ordner "Database"
        return Path.Combine(basePath, "Database", "hackerspace.db");
    }
}