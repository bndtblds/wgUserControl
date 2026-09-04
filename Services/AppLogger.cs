namespace WgUserControl.Services;

internal sealed class AppLogger : IDisposable
{
    private readonly object sync = new();
    private readonly StreamWriter writer;

    private AppLogger(StreamWriter writer)
    {
        this.writer = writer;
        this.writer.AutoFlush = true;
    }

    public static AppLogger CreateDefault()
    {
        Directory.CreateDirectory(AppPaths.LogsDirectory);
        RotateLogs();
        var path = Path.Combine(AppPaths.LogsDirectory, $"wgUserControl-{DateTime.Now:yyyyMMdd}.log");
        return new AppLogger(new StreamWriter(new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)));
    }

    internal static AppLogger CreateForDirectory(string directory)
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "test.log");
        return new AppLogger(new StreamWriter(new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)));
    }

    public void Info(string message) => Write("INFO", message);
    public void Warn(string message) => Write("WARN", message);
    public void Error(string message, Exception? exception = null) => Write("ERROR", exception is null ? message : $"{message} {exception.GetType().Name}: {exception.Message}");

    public void Write(string level, string message)
    {
        lock (sync)
        {
            writer.WriteLine($"{DateTimeOffset.Now:O} [{level}] {Sanitize(message)}");
        }
    }

    public static string Sanitize(string message)
    {
        var lines = message.Split(["\r\n", "\n"], StringSplitOptions.None);
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("PrivateKey", StringComparison.OrdinalIgnoreCase))
            {
                lines[i] = "[redacted WireGuard private key line]";
            }
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static void RotateLogs()
    {
        if (!Directory.Exists(AppPaths.LogsDirectory))
        {
            return;
        }

        foreach (var file in Directory.GetFiles(AppPaths.LogsDirectory, "wgUserControl-*.log")
                     .OrderByDescending(File.GetCreationTimeUtc)
                     .Skip(14))
        {
            try
            {
                File.Delete(file);
            }
            catch
            {
                // Logging must never prevent the application from starting.
            }
        }
    }

    public void Dispose() => writer.Dispose();
}
