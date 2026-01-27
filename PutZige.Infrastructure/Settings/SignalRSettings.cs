namespace PutZige.Infrastructure.Settings;

public sealed class SignalRSettings
{
    public const string SectionName = "SignalRSettings";

    public bool EnableRedis { get; init; } = false;
    public string? RedisConnectionString { get; init; }
    public int KeepAliveIntervalSeconds { get; init; } = 15;
    public int ClientTimeoutSeconds { get; init; } = 30;
    public int HandshakeTimeoutSeconds { get; init; } = 15;
}
