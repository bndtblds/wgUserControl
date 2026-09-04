using System.Windows.Forms;

namespace WgUserControl.Services;

internal sealed class AppInstaller
{
    private readonly StartupManager startup;
    private readonly AppLogger logger;

    public AppInstaller(StartupManager startup, AppLogger logger)
    {
        this.startup = startup;
        this.logger = logger;
    }

    public static bool IsInstalled()
    {
        var current = Environment.ProcessPath ?? Application.ExecutablePath;
        return File.Exists(AppPaths.InstalledExePath)
            && string.Equals(Path.GetFullPath(current), Path.GetFullPath(AppPaths.InstalledExePath), StringComparison.OrdinalIgnoreCase);
    }

    public int InstallApp(string? sourcePath)
    {
        var source = string.IsNullOrWhiteSpace(sourcePath) ? Environment.ProcessPath ?? Application.ExecutablePath : sourcePath;
        Directory.CreateDirectory(AppPaths.InstallDirectory);
        Directory.CreateDirectory(AppPaths.ProgramDataRoot);
        File.Copy(source, AppPaths.InstalledExePath, overwrite: true);
        startup.EnableForAllInteractiveUsers();
        logger.Info($"Application installed to '{AppPaths.InstalledExePath}'.");
        MessageBox.Show(UiText.Get("Installed"), "wgUserControl", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return 0;
    }

    public int UninstallApp()
    {
        startup.DisableForAllInteractiveUsers();
        logger.Info("Application uninstall requested. Installed executable is left for external removal if currently running.");
        MessageBox.Show(UiText.Get("Uninstalled"), "wgUserControl", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return 0;
    }
}
