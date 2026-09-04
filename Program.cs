using System.Windows.Forms;
using WgUserControl.Services;
using WgUserControl.UI;

namespace WgUserControl;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        using var logger = AppLogger.CreateDefault();
        logger.Info("wgUserControl started.");

        try
        {
            var command = CommandLine.Parse(args);
            var repository = TunnelRepository.CreateDefault(logger);
            var serviceManager = new TunnelServiceManager(logger);
            var wireGuard = WireGuardService.CreateDefault(logger);
            var security = new ServiceSecurityManager(logger);
            var configSecurity = new ConfigSecurityManager(logger);
            var startup = StartupManager.CreateDefault(logger);
            var elevation = new ElevationManager(logger);
            var installer = new TunnelInstaller(repository, wireGuard, serviceManager, security, configSecurity, logger);
            var appInstaller = new AppInstaller(startup, logger);

            return command.Mode switch
            {
                AppMode.InstallApp => RequireAdmin(command, elevation, () => appInstaller.InstallApp(command.SourcePath)),
                AppMode.UninstallApp => RequireAdmin(command, elevation, appInstaller.UninstallApp),
                AppMode.Import => RequireAdmin(command, elevation, () => ImportFromCli(command, installer)),
                AppMode.Remove => RequireAdmin(command, elevation, () => RemoveFromCli(command, installer)),
                AppMode.Rename => RequireAdmin(command, elevation, () => RenameFromCli(command, installer)),
                AppMode.Repair => RequireAdmin(command, elevation, () => RepairFromCli(command, repository, security, serviceManager, logger)),
                AppMode.Manage => RunManagement(repository, serviceManager, installer, elevation, logger),
                AppMode.Tray => RunTray(repository, serviceManager, elevation, logger),
                AppMode.Help => ShowHelp(),
                _ => RunDefault(repository, serviceManager, installer, elevation, logger)
            };
        }
        catch (Exception ex)
        {
            logger.Error("Fatal error.", ex);
            MessageBox.Show(ex.Message, "wgUserControl", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 1;
        }
    }

    private static int RunDefault(TunnelRepository repository, TunnelServiceManager serviceManager, TunnelInstaller installer, ElevationManager elevation, AppLogger logger)
    {
        if (AppInstaller.IsInstalled())
        {
            return RunTray(repository, serviceManager, elevation, logger);
        }

        return RunManagement(repository, serviceManager, installer, elevation, logger);
    }

    private static int RunTray(TunnelRepository repository, TunnelServiceManager serviceManager, ElevationManager elevation, AppLogger logger)
    {
        Application.Run(new TrayApplicationContext(repository, serviceManager, elevation, logger));
        return 0;
    }

    private static int RunManagement(TunnelRepository repository, TunnelServiceManager serviceManager, TunnelInstaller installer, ElevationManager elevation, AppLogger logger)
    {
        Application.Run(new ManagementForm(repository, serviceManager, installer, elevation, logger));
        return 0;
    }

    private static int RequireAdmin(CommandLine command, ElevationManager elevation, Func<int> action)
    {
        if (ElevationManager.IsCurrentProcessElevated())
        {
            return action();
        }

        return elevation.RestartElevated(command.OriginalArgs) ? 0 : 1223;
    }

    private static int ImportFromCli(CommandLine command, TunnelInstaller installer)
    {
        if (string.IsNullOrWhiteSpace(command.Target))
        {
            throw new InvalidOperationException(UiText.Get("MissingConfigPath"));
        }

        var displayName = command.DisplayName ?? Path.GetFileNameWithoutExtension(command.Target);
        installer.ImportTunnel(command.Target, displayName);
        return 0;
    }

    private static int RemoveFromCli(CommandLine command, TunnelInstaller installer)
    {
        if (string.IsNullOrWhiteSpace(command.Target))
        {
            throw new InvalidOperationException(UiText.Get("MissingTunnelIdentifier"));
        }

        installer.RemoveTunnel(command.Target);
        return 0;
    }

    private static int RenameFromCli(CommandLine command, TunnelInstaller installer)
    {
        if (string.IsNullOrWhiteSpace(command.Target) || string.IsNullOrWhiteSpace(command.DisplayName))
        {
            throw new InvalidOperationException(UiText.Get("MissingRenameArguments"));
        }

        installer.RenameTunnel(command.Target, command.DisplayName);
        return 0;
    }

    private static int RepairFromCli(CommandLine command, TunnelRepository repository, ServiceSecurityManager security, TunnelServiceManager serviceManager, AppLogger logger)
    {
        var tunnels = repository.Load();
        var selected = string.IsNullOrWhiteSpace(command.Target)
            ? tunnels
            : tunnels.Where(t => t.Id.Equals(command.Target, StringComparison.OrdinalIgnoreCase)
                || t.TechnicalName.Equals(command.Target, StringComparison.OrdinalIgnoreCase)
                || t.ServiceName.Equals(command.Target, StringComparison.OrdinalIgnoreCase)).ToList();

        foreach (var tunnel in selected)
        {
            if (!serviceManager.ServiceExists(tunnel.ServiceName))
            {
                logger.Warn($"Cannot repair missing service '{tunnel.ServiceName}'.");
                continue;
            }

            security.EnsureInteractiveUsersCanOperateTunnel(tunnel.ServiceName);
        }

        return 0;
    }

    private static int ShowHelp()
    {
        MessageBox.Show(UiText.Get("Help"), "wgUserControl", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return 0;
    }
}
