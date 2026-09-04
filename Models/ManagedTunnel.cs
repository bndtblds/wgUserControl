namespace WgUserControl.Models;

internal sealed class ManagedTunnel
{
    public required string Id { get; init; }
    public required string DisplayName { get; set; }
    public required string TechnicalName { get; init; }
    public required string ServiceName { get; init; }
    public required string ConfigPath { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
