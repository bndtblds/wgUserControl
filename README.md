# wgUserControl

`wgUserControl` is a lightweight Windows tray application for controlling prepared WireGuard tunnels as a standard interactive user.

It is intended for environments where users should be able to see, start, stop, and check the status of selected WireGuard tunnels without being local administrators, without being members of Network Configuration Operators, and without being able to read WireGuard private keys.

## Requirements

- Windows with the official WireGuard client installed.
- WireGuard installed in its default location.
- A built or published `wgUserControl.exe`.
- Administrator rights for setup and tunnel management.
- No administrator rights for normal tray usage.

## Build

A .NET 8 SDK with Windows Desktop support is required.

```text
dotnet build wgUserControl.sln
dotnet test wgUserControl.sln
dotnet publish wgUserControl.csproj -c Release
```

Use the published `wgUserControl.exe` for installation or portable testing.

## Typical Workflow

1. Install wgUserControl once as an administrator.
2. Open the management UI and import one or more WireGuard `.conf` files.
3. Let wgUserControl start automatically in the tray when users log on.
4. Start and stop imported tunnels from the tray menu as a standard user.

After setup, day-to-day usage happens from the tray icon.

## Install wgUserControl

Run the executable once as an administrator. Installation configures wgUserControl to start automatically in the tray when a user logs on. No separate autostart setup is required.

After installation, sign out and sign back in. wgUserControl will appear in the Windows notification area.

You can also install it directly:

```text
wgUserControl.exe --install-app
```

## Open the Management UI

The management UI is used to import, remove, rename, or repair tunnels.

Open it from the tray:

1. Right-click the wgUserControl tray icon.
2. Choose `Manage...`.

The management UI can show tunnel status without elevation. When an administrative action is started, Windows asks for administrator approval.

You can also open it directly:

```text
wgUserControl.exe --manage
```

## Import a Tunnel

Import tunnels from the management UI:

1. Open the management UI from the tray menu with `Manage...`.
2. Choose `Import`.
3. Select the WireGuard `.conf` file.
4. Confirm or edit the display name.
5. Approve the administrator prompt.

The imported tunnel appears in the tray menu after the import succeeds.

wgUserControl does not show WireGuard configuration contents and does not expose private keys to standard users.

You can also import a tunnel directly:

```text
wgUserControl.exe --import <conf> [--name <displayName>]
```

## Use the Tray Menu

Right-click the tray icon to open the tunnel menu.

Managed tunnels are shown with a status symbol:

```text
● running
○ stopped
◐ starting
◑ stopping
? unknown or error
```

Click a stopped tunnel to start it. Click a running tunnel to stop it.

Starting and stopping imported tunnels does not require administrator rights. If Windows denies the operation, repair the tunnel permissions from the management UI.

Left-click the tray icon to show a short status overview.

You can also start tray mode directly:

```text
wgUserControl.exe --tray
```

## Manage Tunnels

Open the management UI from the tray menu with `Manage...`.

Available actions:

- `Import`: import a new WireGuard `.conf` file.
- `Remove`: remove an imported tunnel.
- `Rename`: change the display name shown in wgUserControl.
- `Repair`: reapply the permissions required for tray start and stop.
- `Close`: close the management window.

Administrative actions show a Windows administrator prompt when needed.

You can also open the management UI directly:

```text
wgUserControl.exe --manage
```

## Remove a Tunnel

Remove tunnels from the management UI:

1. Open the management UI from the tray menu with `Manage...`.
2. Select the tunnel.
3. Choose `Remove`.
4. Confirm the prompt.
5. Approve the administrator prompt.

Removing a tunnel stops it if needed and removes the imported tunnel from wgUserControl.

You can also remove a tunnel directly:

```text
wgUserControl.exe --remove <id|technicalName|serviceName>
```

## Rename a Tunnel

Rename tunnels from the management UI:

1. Open the management UI from the tray menu with `Manage...`.
2. Select the tunnel.
3. Choose `Rename`.
4. Enter the new display name.
5. Approve the administrator prompt.

Renaming changes only the name shown by wgUserControl.

You can also rename a tunnel directly:

```text
wgUserControl.exe --rename <id|technicalName|serviceName> --name <displayName>
```

## Repair Tunnel Permissions

Repair tunnel permissions from the management UI:

1. Open the management UI from the tray menu with `Manage...`.
2. Select the tunnel.
3. Choose `Repair`.
4. Approve the administrator prompt.

Use repair if a standard user can see a tunnel but cannot start or stop it.

You can also repair tunnel permissions directly:

```text
wgUserControl.exe --repair [id|technicalName|serviceName]
```

## Uninstall wgUserControl

Remove imported tunnels from the management UI first if you also want to remove them.

Then remove the application autostart entry. This does not remove imported tunnels by itself.

After removing the autostart entry, close wgUserControl and delete the installed executable.

You can also remove the autostart entry directly:

```text
wgUserControl.exe --uninstall-app
```

## Command Line Reference

The GUI is the primary way to use wgUserControl. The same operations are also available from the command line:

```text
wgUserControl.exe --install-app [sourceExe]
wgUserControl.exe --uninstall-app
wgUserControl.exe --import <conf> [--name <displayName>]
wgUserControl.exe --remove <id|technicalName|serviceName>
wgUserControl.exe --rename <id|technicalName|serviceName> --name <displayName>
wgUserControl.exe --repair [id|technicalName|serviceName]
wgUserControl.exe --manage
wgUserControl.exe --tray
```

## Files and Locations

Installed application:

```text
C:\Program Files\wgUserControl\wgUserControl.exe
```

Application data:

```text
C:\ProgramData\wgUserControl\
```

Logs:

```text
C:\ProgramData\wgUserControl\Logs\
```

## Security Notes

- Normal tray usage runs without elevation.
- wgUserControl does not install its own privileged background service.
- wgUserControl does not grant users general network configuration rights.
- wgUserControl does not require membership in Network Configuration Operators.
- WireGuard configuration contents are never shown in the UI.
- Private keys and full configuration contents are not written to logs.
- Only tunnels imported by wgUserControl are shown and managed.

## Troubleshooting

If wgUserControl does not start at logon, run `--install-app` again as an administrator.

If the tray icon is not visible, check the Windows notification area overflow menu.

If WireGuard is not found, install the official WireGuard client in its default location.

If starting or stopping a tunnel is denied, open the management UI, select the tunnel, and choose `Repair`.

If a tunnel does not appear in the tray, import it through the management UI.

If metadata is damaged, wgUserControl backs up the damaged metadata file and starts with an empty tunnel list. Existing tunnels are not removed automatically.

If WireGuard was removed after tunnels were imported, reinstall WireGuard before importing, removing, or repairing tunnels.
