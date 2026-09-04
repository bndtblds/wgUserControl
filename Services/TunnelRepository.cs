using System.Text.Json;
using WgUserControl.Models;

namespace WgUserControl.Services;

internal sealed class TunnelRepository
{
    private readonly string metadataPath;
    private readonly AppLogger logger;
    private readonly JsonSerializerOptions jsonOptions = new() { WriteIndented = true };

    public TunnelRepository(string metadataPath, AppLogger logger)
    {
        this.metadataPath = metadataPath;
        this.logger = logger;
    }

    public static TunnelRepository CreateDefault(AppLogger logger) => new(AppPaths.MetadataPath, logger);

    public List<ManagedTunnel> Load()
    {
        if (!File.Exists(metadataPath))
        {
            return [];
        }

        try
        {
            var json = File.ReadAllText(metadataPath);
            return JsonSerializer.Deserialize<List<ManagedTunnel>>(json, jsonOptions) ?? [];
        }
        catch (Exception ex)
        {
            logger.Error("Tunnel metadata is damaged.", ex);
            var backup = metadataPath + ".broken-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            File.Copy(metadataPath, backup, overwrite: false);
            return [];
        }
    }

    public void Save(IReadOnlyCollection<ManagedTunnel> tunnels)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(metadataPath)!);
        var json = JsonSerializer.Serialize(tunnels.OrderBy(t => t.DisplayName).ThenBy(t => t.Id), jsonOptions);
        File.WriteAllText(metadataPath, json);
    }

    public ManagedTunnel? Find(string idOrName)
    {
        return Load().FirstOrDefault(t =>
            t.Id.Equals(idOrName, StringComparison.OrdinalIgnoreCase)
            || t.TechnicalName.Equals(idOrName, StringComparison.OrdinalIgnoreCase)
            || t.ServiceName.Equals(idOrName, StringComparison.OrdinalIgnoreCase));
    }

    public void Upsert(ManagedTunnel tunnel)
    {
        var tunnels = Load();
        tunnels.RemoveAll(t => t.Id.Equals(tunnel.Id, StringComparison.OrdinalIgnoreCase));
        tunnels.Add(tunnel);
        Save(tunnels);
    }

    public void Remove(string id)
    {
        var tunnels = Load();
        tunnels.RemoveAll(t => t.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        Save(tunnels);
    }
}
