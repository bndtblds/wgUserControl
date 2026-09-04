using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace WgUserControl.Services;

internal static partial class TunnelNameService
{
    public static string CreateId()
    {
        Span<byte> bytes = stackalloc byte[4];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes);
    }

    public static string CreateTechnicalName(string displayName, string id)
    {
        var safeName = SanitizeForTechnicalName(displayName);
        return $"{AppPaths.TunnelPrefix}{safeName}_{id.ToUpperInvariant()}";
    }

    public static string CreateServiceName(string technicalName) => $"WireGuardTunnel${technicalName}";

    public static bool IsManagedServiceName(string serviceName) =>
        serviceName.StartsWith(AppPaths.ServicePrefix, StringComparison.OrdinalIgnoreCase);

    public static string SanitizeForTechnicalName(string displayName)
    {
        var value = Transliterate(displayName).Trim();
        value = InvalidTechnicalNameCharacters().Replace(value, "_");
        value = RepeatedUnderscores().Replace(value, "_").Trim('_');
        return string.IsNullOrWhiteSpace(value) ? "Tunnel" : value;
    }

    private static string Transliterate(string value)
    {
        value = value
            .Replace("Ä", "Ae", StringComparison.Ordinal)
            .Replace("Ö", "Oe", StringComparison.Ordinal)
            .Replace("Ü", "Ue", StringComparison.Ordinal)
            .Replace("ä", "ae", StringComparison.Ordinal)
            .Replace("ö", "oe", StringComparison.Ordinal)
            .Replace("ü", "ue", StringComparison.Ordinal)
            .Replace("ß", "ss", StringComparison.Ordinal);

        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(ch);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    [GeneratedRegex("[^A-Za-z0-9_.-]+")]
    private static partial Regex InvalidTechnicalNameCharacters();

    [GeneratedRegex("_+")]
    private static partial Regex RepeatedUnderscores();
}
