namespace PutZige.Infrastructure.Settings;

public sealed class DatabaseSettings
{
    public const string SectionName = "Database";

    public required string ConnectionString { get; init; }

    public int MaxRetryCount { get; init; } = 5;

    public int CommandTimeout { get; init; } = 30;

    public bool EnableSensitiveDataLogging { get; init; } = false;

    public bool EnableDetailedErrors { get; init; } = false;
}