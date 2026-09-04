namespace WgUserControl.Services;

internal static class AppPaths
{
    public const string AppName = "wgUserControl";
    public const string TunnelPrefix = "wgUserControl_";
    public const string ServicePrefix = "WireGuardTunnel$wgUserControl_";

    public static string ProgramDataRoot { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        AppName);

    public static string LogsDirectory { get; } = Path.Combine(ProgramDataRoot, "Logs");
    public static string MetadataPath { get; } = Path.Combine(ProgramDataRoot, "tunnels.json");
    public static string InstallDirectory { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), AppName);
    public static string InstalledExePath { get; } = Path.Combine(InstallDirectory, "wgUserControl.exe");
}
