using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WgUserControl.Models;
using WgUserControl.Services;

namespace WgUserControl.UI;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly TunnelRepository repository;
    private readonly TunnelServiceManager serviceManager;
    private readonly ElevationManager elevation;
    private readonly AppLogger logger;
    private readonly NotifyIcon notifyIcon;
    private readonly System.Windows.Forms.Timer refreshTimer = new() { Interval = 5000 };

    public TrayApplicationContext(TunnelRepository repository, TunnelServiceManager serviceManager, ElevationManager elevation, AppLogger logger)
    {
        this.repository = repository;
        this.serviceManager = serviceManager;
        this.elevation = elevation;
        this.logger = logger;
        notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "wgUserControl",
            Visible = true
        };

        notifyIcon.MouseClick += (_, args) =>
        {
            if (args.Button == MouseButtons.Left)
            {
                notifyIcon.ShowBalloonTip(2000, "wgUserControl", BuildTooltipStatus(), ToolTipIcon.Info);
            }
        };

        refreshTimer.Tick += (_, _) => BuildMenu();
        refreshTimer.Start();
        BuildMenu();
    }

    private void BuildMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("wgUserControl").Enabled = false;
        menu.Items.Add(new ToolStripSeparator());

        var tunnels = repository.Load();
        if (tunnels.Count == 0)
        {
            menu.Items.Add(UiText.Get("NoTunnels")).Enabled = false;
        }

        foreach (var tunnel in tunnels)
        {
            var status = SafeStatus(tunnel);
            var item = new ToolStripMenuItem($"{StatusSymbol(status)} {tunnel.DisplayName}")
            {
                Tag = tunnel,
                Enabled = status is TunnelRuntimeStatus.Running or TunnelRuntimeStatus.Stopped
            };
            item.Click += async (_, _) => await ToggleTunnelAsync(tunnel);
            menu.Items.Add(item);
        }

        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(UiText.Get("Manage"), null, (_, _) => OpenManagement());
        menu.Items.Add(UiText.Get("Info"), null, (_, _) => MessageBox.Show("wgUserControl", "wgUserControl", MessageBoxButtons.OK, MessageBoxIcon.Information));
        menu.Items.Add(UiText.Get("Exit"), null, (_, _) => ExitThread());

        notifyIcon.ContextMenuStrip = menu;
        notifyIcon.Text = "wgUserControl";
    }

    private async Task ToggleTunnelAsync(ManagedTunnel tunnel)
    {
        try
        {
            await Task.Run(() =>
            {
                var status = serviceManager.GetStatus(tunnel.ServiceName);
                if (status == TunnelRuntimeStatus.Running)
                {
                    serviceManager.Stop(tunnel.ServiceName);
                }
                else if (status == TunnelRuntimeStatus.Stopped)
                {
                    serviceManager.Start(tunnel.ServiceName);
                }
            });
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == NativeMethods.ErrorAccessDenied)
        {
            MessageBox.Show(UiText.Get("AccessDenied"), "wgUserControl", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            logger.Error($"Start/Stop failed for {tunnel.ServiceName}.", ex);
            MessageBox.Show(ex.Message, "wgUserControl", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            BuildMenu();
        }
    }

    private TunnelRuntimeStatus SafeStatus(ManagedTunnel tunnel)
    {
        try
        {
            return serviceManager.GetStatus(tunnel.ServiceName);
        }
        catch
        {
            return TunnelRuntimeStatus.Error;
        }
    }

    private string BuildTooltipStatus()
    {
        var tunnels = repository.Load();
        if (tunnels.Count == 0)
        {
            return UiText.Get("NoManagedTunnels");
        }

        return string.Join(Environment.NewLine, tunnels.Select(t => $"{t.DisplayName}: {SafeStatus(t)}"));
    }

    private void OpenManagement()
    {
        var exe = File.Exists(AppPaths.InstalledExePath) ? AppPaths.InstalledExePath : Environment.ProcessPath ?? Application.ExecutablePath;
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(exe)
        {
            UseShellExecute = true,
            Arguments = "--manage"
        });
    }

    private static string StatusSymbol(TunnelRuntimeStatus status) => status switch
    {
        TunnelRuntimeStatus.Running => "●",
        TunnelRuntimeStatus.Stopped => "○",
        TunnelRuntimeStatus.StartPending => "◐",
        TunnelRuntimeStatus.StopPending => "◑",
        _ => "?"
    };

    protected override void ExitThreadCore()
    {
        refreshTimer.Stop();
        notifyIcon.Visible = false;
        notifyIcon.Dispose();
        base.ExitThreadCore();
    }
}
