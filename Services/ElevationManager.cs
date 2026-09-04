using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;

namespace WgUserControl.Services;

internal sealed class ElevationManager
{
    private const int ErrorCancelled = 1223;
    private readonly AppLogger logger;

    public ElevationManager(AppLogger logger)
    {
        this.logger = logger;
    }

    public static bool IsCurrentProcessElevated()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public bool RestartElevated(params string[] args)
    {
        try
        {
            var exe = Environment.ProcessPath ?? Application.ExecutablePath;
            Process.Start(new ProcessStartInfo(exe)
            {
                UseShellExecute = true,
                Verb = "runas",
                Arguments = QuoteArguments(args)
            });
            return true;
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == ErrorCancelled)
        {
            logger.Warn("Elevation was cancelled.");
            return false;
        }
    }

    public bool RunInstalledElevated(params string[] args)
    {
        try
        {
            var exe = File.Exists(AppPaths.InstalledExePath) ? AppPaths.InstalledExePath : Environment.ProcessPath ?? Application.ExecutablePath;
            Process.Start(new ProcessStartInfo(exe)
            {
                UseShellExecute = true,
                Verb = "runas",
                Arguments = QuoteArguments(args)
            });
            return true;
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == ErrorCancelled)
        {
            logger.Warn("Elevation was cancelled.");
            return false;
        }
    }

    private static string QuoteArguments(IEnumerable<string> args) =>
        string.Join(" ", args.Select(arg => "\"" + arg.Replace("\"", "\\\"", StringComparison.Ordinal) + "\""));
}
