using System.Drawing;
using System.Windows.Forms;
using WgUserControl.Models;
using WgUserControl.Services;

namespace WgUserControl.UI;

internal sealed class ManagementForm : Form
{
    private readonly TunnelRepository repository;
    private readonly TunnelServiceManager serviceManager;
    private readonly TunnelInstaller installer;
    private readonly ElevationManager elevation;
    private readonly AppLogger logger;
    private readonly ListView listView = new();

    public ManagementForm(TunnelRepository repository, TunnelServiceManager serviceManager, TunnelInstaller installer, ElevationManager elevation, AppLogger logger)
    {
        this.repository = repository;
        this.serviceManager = serviceManager;
        this.installer = installer;
        this.elevation = elevation;
        this.logger = logger;

        Text = "wgUserControl";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(760, 420);
        Icon = SystemIcons.Shield;

        listView.Dock = DockStyle.Fill;
        listView.View = View.Details;
        listView.FullRowSelect = true;
        listView.Columns.Add(UiText.Get("Name"), 180);
        listView.Columns.Add(UiText.Get("Status"), 110);
        listView.Columns.Add(UiText.Get("TechnicalName"), 240);
        listView.Columns.Add(UiText.Get("Configuration"), 420);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 44, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8) };
        buttons.Controls.Add(CreateButton(UiText.Get("Close"), (_, _) => Close()));
        buttons.Controls.Add(CreateButton(UiText.Get("Repair"), (_, _) => RepairSelected()));
        buttons.Controls.Add(CreateButton(UiText.Get("Rename"), (_, _) => RenameSelected()));
        buttons.Controls.Add(CreateButton(UiText.Get("Remove"), (_, _) => RemoveSelected()));
        buttons.Controls.Add(CreateButton(UiText.Get("Import"), (_, _) => ImportTunnel()));

        Controls.Add(listView);
        Controls.Add(buttons);

        Load += (_, _) => RefreshList();
    }

    private static Button CreateButton(string text, EventHandler click)
    {
        var button = new Button { Text = text, Width = 105, Height = 28 };
        button.Click += click;
        return button;
    }

    private void RefreshList()
    {
        listView.Items.Clear();
        foreach (var tunnel in repository.Load())
        {
            var status = SafeStatus(tunnel);
            var item = new ListViewItem([tunnel.DisplayName, status.ToString(), tunnel.TechnicalName, tunnel.ConfigPath])
            {
                Tag = tunnel
            };
            listView.Items.Add(item);
        }
    }

    private TunnelRuntimeStatus SafeStatus(ManagedTunnel tunnel)
    {
        try
        {
            return serviceManager.GetStatus(tunnel.ServiceName);
        }
        catch (Exception ex)
        {
            logger.Error($"Could not read status for {tunnel.ServiceName}.", ex);
            return TunnelRuntimeStatus.Error;
        }
    }

    private ManagedTunnel? SelectedTunnel() => listView.SelectedItems.Count == 0 ? null : (ManagedTunnel?)listView.SelectedItems[0].Tag;

    private void ImportTunnel()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = UiText.Get("ConfigFilter"),
            CheckFileExists = true
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var name = InputDialog.Show(this, UiText.Get("ImportTunnel"), UiText.Get("DisplayName"), Path.GetFileNameWithoutExtension(dialog.FileName));
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        if (!ElevationManager.IsCurrentProcessElevated())
        {
            elevation.RunInstalledElevated("--import", dialog.FileName, "--name", name);
            return;
        }

        installer.ImportTunnel(dialog.FileName, name);
        RefreshList();
    }

    private void RemoveSelected()
    {
        var tunnel = SelectedTunnel();
        if (tunnel is null)
        {
            return;
        }

        if (MessageBox.Show(this, string.Format(UiText.Get("RemoveQuestion"), tunnel.DisplayName), "wgUserControl", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
        {
            return;
        }

        if (!ElevationManager.IsCurrentProcessElevated())
        {
            elevation.RunInstalledElevated("--remove", tunnel.Id);
            return;
        }

        installer.RemoveTunnel(tunnel.Id);
        RefreshList();
    }

    private void RenameSelected()
    {
        var tunnel = SelectedTunnel();
        if (tunnel is null)
        {
            return;
        }

        var name = InputDialog.Show(this, UiText.Get("RenameTunnel"), UiText.Get("DisplayName"), tunnel.DisplayName);
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        if (!ElevationManager.IsCurrentProcessElevated())
        {
            elevation.RunInstalledElevated("--rename", tunnel.Id, "--name", name);
            return;
        }

        installer.RenameTunnel(tunnel.Id, name);
        RefreshList();
    }

    private void RepairSelected()
    {
        var tunnel = SelectedTunnel();
        if (tunnel is null)
        {
            return;
        }

        if (!ElevationManager.IsCurrentProcessElevated())
        {
            elevation.RunInstalledElevated("--repair", tunnel.Id);
            return;
        }

        new ServiceSecurityManager(logger).EnsureInteractiveUsersCanOperateTunnel(tunnel.ServiceName);
        RefreshList();
    }
}
