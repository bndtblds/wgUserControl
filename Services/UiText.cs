using System.Globalization;

namespace WgUserControl.Services;

internal static class UiText
{
    private static bool German => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("de", StringComparison.OrdinalIgnoreCase);

    public static string Get(string key) => German ? GermanText(key) : EnglishText(key);

    private static string EnglishText(string key) => key switch
    {
        "Installed" => "wgUserControl has been installed.",
        "Uninstalled" => "Autostart has been removed. Remove the EXE from Program Files after closing the application.",
        "MissingConfigPath" => "Please specify a WireGuard configuration path.",
        "MissingTunnelIdentifier" => "Please specify an ID or technical tunnel name.",
        "MissingRenameArguments" => "Please specify an ID and a new display name.",
        "MissingConfigFile" => "The WireGuard configuration file does not exist.",
        "ForeignTunnelRemoveDenied" => "Foreign WireGuard services are not removed.",
        "TunnelNotFound" => "The specified wgUserControl tunnel was not found.",
        "WireGuardNotFound" => "WireGuard was not found at 'C:\\Program Files\\WireGuard\\wireguard.exe'.",
        "AbsoluteConfigPathRequired" => "WireGuard requires an absolute path to the tunnel configuration.",
        "WireGuardStartFailed" => "WireGuard could not be started.",
        "WireGuardFailed" => "WireGuard reported an error:",
        "ExpectedServiceMissing" => "The expected service was not created:",
        "ServiceNotLocalSystem" => "The WireGuard tunnel service does not run as LocalSystem. Import cancelled.",
        "NoTunnels" => "No tunnels",
        "NoManagedTunnels" => "No managed tunnels",
        "Manage" => "Manage...",
        "Info" => "About",
        "Exit" => "Exit",
        "Name" => "Name",
        "Status" => "Status",
        "TechnicalName" => "Technical name",
        "Configuration" => "Configuration",
        "Close" => "Close",
        "Repair" => "Repair",
        "Rename" => "Rename",
        "Remove" => "Remove",
        "Import" => "Import",
        "ConfigFilter" => "WireGuard configuration (*.conf)|*.conf|All files (*.*)|*.*",
        "ImportTunnel" => "Import tunnel",
        "DisplayName" => "Display name",
        "RenameTunnel" => "Rename tunnel",
        "RemoveQuestion" => "Remove tunnel '{0}'?",
        "AccessDenied" => "Start/stop was denied. The service permissions may not be configured correctly.",
        "Help" => """
            wgUserControl

            --install-app [sourceExe]
            --uninstall-app
            --import <conf> [--name <displayName>]
            --remove <id|technicalName|serviceName>
            --rename <id|technicalName|serviceName> --name <displayName>
            --repair [id|technicalName|serviceName]
            --manage
            --tray
            """,
        _ => key
    };

    private static string GermanText(string key) => key switch
    {
        "Installed" => "wgUserControl wurde installiert.",
        "Uninstalled" => "Autostart wurde entfernt. Entferne die EXE nach dem Beenden aus Program Files.",
        "MissingConfigPath" => "Bitte Pfad zur WireGuard-Konfiguration angeben.",
        "MissingTunnelIdentifier" => "Bitte ID oder technischen Tunnelnamen angeben.",
        "MissingRenameArguments" => "Bitte ID und neuen Anzeigenamen angeben.",
        "MissingConfigFile" => "Die WireGuard-Konfigurationsdatei existiert nicht.",
        "ForeignTunnelRemoveDenied" => "Fremde WireGuard-Dienste werden nicht entfernt.",
        "TunnelNotFound" => "Der angegebene wgUserControl-Tunnel wurde nicht gefunden.",
        "WireGuardNotFound" => "WireGuard wurde nicht unter 'C:\\Program Files\\WireGuard\\wireguard.exe' gefunden.",
        "AbsoluteConfigPathRequired" => "WireGuard benötigt einen absoluten Pfad zur Tunnel-Konfiguration.",
        "WireGuardStartFailed" => "WireGuard konnte nicht gestartet werden.",
        "WireGuardFailed" => "WireGuard meldete einen Fehler:",
        "ExpectedServiceMissing" => "Der erwartete Dienst wurde nicht erstellt:",
        "ServiceNotLocalSystem" => "Der WireGuard-Tunnel-Service läuft nicht als LocalSystem. Import abgebrochen.",
        "NoTunnels" => "Keine Tunnel",
        "NoManagedTunnels" => "Keine verwalteten Tunnel",
        "Manage" => "Verwalten...",
        "Info" => "Info",
        "Exit" => "Beenden",
        "Name" => "Name",
        "Status" => "Status",
        "TechnicalName" => "Technischer Name",
        "Configuration" => "Konfiguration",
        "Close" => "Schließen",
        "Repair" => "Reparieren",
        "Rename" => "Umbenennen",
        "Remove" => "Entfernen",
        "Import" => "Importieren",
        "ConfigFilter" => "WireGuard-Konfiguration (*.conf)|*.conf|Alle Dateien (*.*)|*.*",
        "ImportTunnel" => "Tunnel importieren",
        "DisplayName" => "Anzeigename",
        "RenameTunnel" => "Tunnel umbenennen",
        "RemoveQuestion" => "Tunnel '{0}' entfernen?",
        "AccessDenied" => "Start/Stop wurde verweigert. Die Dienstberechtigungen sind möglicherweise nicht korrekt eingerichtet.",
        "Help" => EnglishText("Help"),
        _ => EnglishText(key)
    };
}
