using HackerspaceLogic.Core;
using Xunit;

namespace HackerspaceTests;

public class FullPipelineTest
{
    [Fact]
    public async Task Download_Validate_ShowDbPath()
    {
        await SpaceDataDownloader.DownloadRawAsync();
        await SpaceDataValidator.ValidateAndStoreAsync();

        string dbPath = Path.Combine("Database", "hackerspace.db");
        Assert.True(File.Exists(dbPath));

        Console.WriteLine($"📦 DB gespeichert unter: {Path.GetFullPath(dbPath)}");
    }
}