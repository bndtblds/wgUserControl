using Microsoft.VisualStudio.TestTools.UnitTesting;
using WgUserControl.Models;
using WgUserControl.Services;

namespace WgUserControl.Tests;

[TestClass]
public sealed class TunnelRepositoryTests
{
    [TestMethod]
    public void SavesAndLoadsMetadata()
    {
        var directory = CreateTempDirectory();
        using var logger = AppLogger.CreateForDirectory(directory);
        var repository = new TunnelRepository(Path.Combine(directory, "tunnels.json"), logger);

        repository.Save([
            new ManagedTunnel
            {
                Id = "12345678",
                DisplayName = "Tunnel 01",
                TechnicalName = "wgUserControl_Tunnel01_12345678",
                ServiceName = "WireGuardTunnel$wgUserControl_Tunnel01_12345678",
                ConfigPath = "C:\\ProgramData\\wgUserControl\\wgUserControl_Tunnel01_12345678.conf"
            }
        ]);

        var loaded = repository.Load();

        Assert.AreEqual(1, loaded.Count);
        Assert.AreEqual("Tunnel 01", loaded[0].DisplayName);
    }

    [TestMethod]
    public void MissingMetadataReturnsEmptyList()
    {
        var directory = CreateTempDirectory();
        using var logger = AppLogger.CreateForDirectory(directory);
        var repository = new TunnelRepository(Path.Combine(directory, "missing.json"), logger);

        Assert.AreEqual(0, repository.Load().Count);
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "wgUserControl-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
