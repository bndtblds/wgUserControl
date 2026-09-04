using WgUserControl.Models;

namespace WgUserControl.Services;

internal sealed class TunnelInstaller
{
    private readonly TunnelRepository repository;
    private readonly WireGuardService wireGuard;
    private readonly TunnelServiceManager serviceManager;
    private readonly ServiceSecurityManager serviceSecurity;
    private readonly ConfigSecurityManager configSecurity;
    private readonly AppLogger logger;

    public TunnelInstaller(
        TunnelRepository repository,
        WireGuardService wireGuard,
        TunnelServiceManager serviceManager,
        ServiceSecurityManager serviceSecurity,
        ConfigSecurityManager configSecurity,
        AppLogger logger)
    {
        this.repository = repository;
        this.wireGuard = wireGuard;
        this.serviceManager = serviceManager;
        this.serviceSecurity = serviceSecurity;
        this.configSecurity = configSecurity;
        this.logger = logger;
    }

    public ManagedTunnel ImportTunnel(string sourceConfigPath, string displayName)
    {
        if (!File.Exists(sourceConfigPath))
        {
            throw new FileNotFoundException(UiText.Get("MissingConfigFile"), sourceConfigPath);
        }

        wireGuard.EnsureAvailable();

        var id = CreateUniqueId();
        var technicalName = TunnelNameService.CreateTechnicalName(displayName, id);
        var serviceName = TunnelNameService.CreateServiceName(technicalName);
        var targetPath = Path.Combine(AppPaths.ProgramDataRoot, technicalName + ".conf");
        var tunnel = new ManagedTunnel
        {
            Id = id,
            DisplayName = displayName,
            TechnicalName = technicalName,
            ServiceName = serviceName,
            ConfigPath = targetPath
        };

        var fileCopied = false;
        var serviceInstalled = false;
        try
        {
            Directory.CreateDirectory(AppPaths.ProgramDataRoot);
            File.Copy(sourceConfigPath, targetPath, overwrite: false);
            fileCopied = true;
            configSecurity.SecureConfigFile(targetPath);

            wireGuard.InstallTunnelService(Path.GetFullPath(targetPath));
            serviceInstalled = true;

            if (!serviceManager.ServiceExists(serviceName))
            {
                throw new InvalidOperationException($"{UiText.Get("ExpectedServiceMissing")} {serviceName}");
            }

            VerifyLocalSystemServiceAccount(serviceName);
            serviceSecurity.EnsureInteractiveUsersCanOperateTunnel(serviceName);
            repository.Upsert(tunnel);
            logger.Info($"Tunnel imported: {tunnel.DisplayName} ({tunnel.TechnicalName})");
            return tunnel;
        }
        catch
        {
            RollbackImport(tunnel, fileCopied, serviceInstalled);
            throw;
        }
    }

    public void RemoveTunnel(string idOrName)
    {
        var tunnel = repository.Find(idOrName)
            ?? throw new InvalidOperationException(UiText.Get("TunnelNotFound"));

        if (!TunnelNameService.IsManagedServiceName(tunnel.ServiceName))
        {
            throw new InvalidOperationException(UiText.Get("ForeignTunnelRemoveDenied"));
        }

        try
        {
            if (serviceManager.ServiceExists(tunnel.ServiceName) && serviceManager.GetStatus(tunnel.ServiceName) == TunnelRuntimeStatus.Running)
            {
                serviceManager.Stop(tunnel.ServiceName);
                Thread.Sleep(1500);
            }

            if (serviceManager.ServiceExists(tunnel.ServiceName))
            {
                wireGuard.UninstallTunnelService(tunnel);
            }

            if (File.Exists(tunnel.ConfigPath))
            {
                File.Delete(tunnel.ConfigPath);
            }

            repository.Remove(tunnel.Id);
            logger.Info($"Tunnel removed: {tunnel.DisplayName} ({tunnel.TechnicalName})");
        }
        catch (Exception ex)
        {
            logger.Error($"Tunnel removal failed for {tunnel.TechnicalName}.", ex);
            throw;
        }
    }

    public void RenameTunnel(string idOrName, string displayName)
    {
        var tunnels = repository.Load();
        var tunnel = tunnels.FirstOrDefault(t =>
            t.Id.Equals(idOrName, StringComparison.OrdinalIgnoreCase)
            || t.TechnicalName.Equals(idOrName, StringComparison.OrdinalIgnoreCase)
            || t.ServiceName.Equals(idOrName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException(UiText.Get("TunnelNotFound"));

        tunnel.DisplayName = displayName;
        repository.Save(tunnels);
        logger.Info($"Tunnel display name changed: {tunnel.TechnicalName}");
    }

    private string CreateUniqueId()
    {
        var existing = repository.Load().Select(t => t.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        string id;
        do
        {
            id = TunnelNameService.CreateId();
        }
        while (existing.Contains(id));

        return id;
    }

    private void VerifyLocalSystemServiceAccount(string serviceName)
    {
        var account = ServiceConfigReader.ReadServiceStartName(serviceName);
        if (!string.Equals(account, "LocalSystem", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"{UiText.Get("ServiceNotLocalSystem")} Account: '{account}'.");
        }
    }

    private void RollbackImport(ManagedTunnel tunnel, bool fileCopied, bool serviceInstalled)
    {
        logger.Warn($"Rolling back tunnel import: {tunnel.TechnicalName}");
        try
        {
            if (serviceInstalled && serviceManager.ServiceExists(tunnel.ServiceName))
            {
                wireGuard.UninstallTunnelService(tunnel);
            }
        }
        catch (Exception ex)
        {
            logger.Error("Rollback failed while uninstalling tunnel service.", ex);
        }

        try
        {
            if (fileCopied && File.Exists(tunnel.ConfigPath))
            {
                File.Delete(tunnel.ConfigPath);
            }
        }
        catch (Exception ex)
        {
            logger.Error("Rollback failed while deleting copied config.", ex);
        }
    }
}
