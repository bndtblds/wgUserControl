using Microsoft.VisualStudio.TestTools.UnitTesting;
using WgUserControl.Services;

namespace WgUserControl.Tests;

[TestClass]
public sealed class TunnelNameServiceTests
{
    [TestMethod]
    public void CreatesTechnicalTunnelNameWithPrefixAndId()
    {
        var name = TunnelNameService.CreateTechnicalName("Tunnel01", "12345678");

        Assert.AreEqual("wgUserControl_Tunnel01_12345678", name);
        Assert.AreEqual("WireGuardTunnel$wgUserControl_Tunnel01_12345678", TunnelNameService.CreateServiceName(name));
    }

    [TestMethod]
    public void SanitizesDisplayNameForWindowsServiceName()
    {
        Assert.AreEqual("AeOeUe_ss_Test", TunnelNameService.SanitizeForTechnicalName("ÄÖÜ ß / Test"));
        Assert.AreEqual("Tunnel", TunnelNameService.SanitizeForTechnicalName("!§$%"));
    }

    [TestMethod]
    public void GeneratedIdsHaveAtLeastEightHexCharacters()
    {
        var id = TunnelNameService.CreateId();

        Assert.AreEqual(8, id.Length);
        StringAssert.Matches(id, new System.Text.RegularExpressions.Regex("^[0-9A-F]{8}$"));
    }

    [TestMethod]
    public void DetectsOnlyManagedWireGuardServices()
    {
        Assert.IsTrue(TunnelNameService.IsManagedServiceName("WireGuardTunnel$wgUserControl_Tunnel01_12345678"));
        Assert.IsFalse(TunnelNameService.IsManagedServiceName("WireGuardTunnel$UnmanagedTunnel01"));
        Assert.IsFalse(TunnelNameService.IsManagedServiceName("OtherService"));
    }
}
