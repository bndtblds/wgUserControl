using Microsoft.Win32;

namespace WgUserControl.Services;

internal sealed class StartupManager
{
    private const string RunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private readonly AppLogger logger;

    private StartupManager(AppLogger logger)
    {
        this.logger = logger;
    }

    public static StartupManager CreateDefault(AppLogger logger) => new(logger);

    public void EnableForAllInteractiveUsers()
    {
        using var key = Registry.LocalMachine.OpenSubKey(RunKey, writable: true) ?? Registry.LocalMachine.CreateSubKey(RunKey, writable: true);
        key.SetValue(AppPaths.AppName, $"\"{AppPaths.InstalledExePath}\" --tray", RegistryValueKind.String);
        logger.Info("HKLM Run autostart configured.");
    }

    public void DisableForAllInteractiveUsers()
    {
        using var key = Registry.LocalMachine.OpenSubKey(RunKey, writable: true);
        key?.DeleteValue(AppPaths.AppName, throwOnMissingValue: false);
        logger.Info("HKLM Run autostart removed.");
    }
}
