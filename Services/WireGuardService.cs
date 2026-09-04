using System.Diagnostics;
using WgUserControl.Models;

namespace WgUserControl.Services;

internal sealed class WireGuardService
{
    private readonly string wireGuardExe;
    private readonly AppLogger logger;

    private WireGuardService(string wireGuardExe, AppLogger logger)
    {
        this.wireGuardExe = wireGuardExe;
        this.logger = logger;
    }

    public static WireGuardService CreateDefault(AppLogger logger)
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "WireGuard", "wireguard.exe");
        return new WireGuardService(path, logger);
    }

    public void EnsureAvailable()
    {
        if (!File.Exists(wireGuardExe))
        {
            throw new FileNotFoundException(UiText.Get("WireGuardNotFound"), wireGuardExe);
        }
    }

    public void InstallTunnelService(string configPath)
    {
        EnsureAvailable();
        if (!Path.IsPathFullyQualified(configPath))
        {
            throw new InvalidOperationException(UiText.Get("AbsoluteConfigPathRequired"));
        }

        RunWireGuard("/installtunnelservice", configPath);
        logger.Info($"WireGuard tunnel service installation requested for '{configPath}'.");
    }

    public void UninstallTunnelService(ManagedTunnel tunnel)
    {
        EnsureAvailable();
        RunWireGuard("/uninstalltunnelservice", tunnel.TechnicalName);
        logger.Info($"WireGuard tunnel service uninstall requested: {tunnel.TechnicalName}");
    }

    private void RunWireGuard(string command, string argument)
    {
        var startInfo = new ProcessStartInfo(wireGuardExe)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };
        startInfo.ArgumentList.Add(command);
        startInfo.ArgumentList.Add(argument);

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException(UiText.Get("WireGuardStartFailed"));

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        process.WaitForExit();
        var stdout = stdoutTask.GetAwaiter().GetResult();
        var stderr = stderrTask.GetAwaiter().GetResult();
        if (process.ExitCode != 0)
        {
            logger.Error($"WireGuard failed: {stderr} {stdout}");
            throw new InvalidOperationException($"{UiText.Get("WireGuardFailed")} {stderr}{stdout}");
        }
    }
}
