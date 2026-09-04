namespace WgUserControl;

internal enum AppMode
{
    Default,
    Tray,
    Manage,
    InstallApp,
    UninstallApp,
    Import,
    Remove,
    Rename,
    Repair,
    Help
}

internal sealed class CommandLine
{
    public required string[] OriginalArgs { get; init; }
    public AppMode Mode { get; init; }
    public string? Target { get; init; }
    public string? DisplayName { get; init; }
    public string? SourcePath { get; init; }

    public static CommandLine Parse(string[] args)
    {
        if (args.Length == 0)
        {
            return new CommandLine { OriginalArgs = args, Mode = AppMode.Default };
        }

        var mode = args[0].ToLowerInvariant() switch
        {
            "--tray" => AppMode.Tray,
            "--manage" => AppMode.Manage,
            "--install-app" => AppMode.InstallApp,
            "--uninstall-app" => AppMode.UninstallApp,
            "--import" => AppMode.Import,
            "--remove" => AppMode.Remove,
            "--rename" => AppMode.Rename,
            "--repair" => AppMode.Repair,
            "--help" or "-h" or "/?" => AppMode.Help,
            _ => AppMode.Help
        };

        var displayName = ReadOption(args, "--name");
        return new CommandLine
        {
            OriginalArgs = args,
            Mode = mode,
            Target = args.Length > 1 && !args[1].StartsWith("--", StringComparison.Ordinal) ? args[1] : null,
            DisplayName = displayName,
            SourcePath = mode == AppMode.InstallApp && args.Length > 1 ? args[1] : null
        };
    }

    private static string? ReadOption(string[] args, string option)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals(option, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        return null;
    }
}
