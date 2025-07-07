using System;
using System.IO;

namespace HackerspaceLogic.Core
{
    public static class DbPathHelper
    {
        public static string GetDatabasePath()
        {
            // Relativer Pfad zur DB innerhalb der Solution (z. B. ChaosMap/Data/)
            var relativePath = Path.Combine("ChaosMap", "Data", "hackerspace.db");
            return Path.GetFullPath(relativePath);
        }
    }
}