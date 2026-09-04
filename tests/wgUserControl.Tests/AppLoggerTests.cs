using Microsoft.VisualStudio.TestTools.UnitTesting;
using WgUserControl.Services;

namespace WgUserControl.Tests;

[TestClass]
public sealed class AppLoggerTests
{
    [TestMethod]
    public void SanitizesPrivateKeyLines()
    {
        var text = """
            [Interface]
            PrivateKey = secret
            Address = 10.0.0.2/32
            """;

        var sanitized = AppLogger.Sanitize(text);

        Assert.IsFalse(sanitized.Contains("secret", StringComparison.Ordinal));
        StringAssert.Contains(sanitized, "Address");
    }
}
