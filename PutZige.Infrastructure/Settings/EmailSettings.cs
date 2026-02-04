namespace PutZige.Infrastructure.Settings;

public sealed class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public required string SmtpHost { get; init; }
    public int SmtpPort { get; init; } = 587;
    public string? SmtpUsername { get; init; }
    public string? SmtpPassword { get; init; }

    public required string FromEmail { get; init; }
    public string? FromName { get; init; }

    public bool EnableSsl { get; init; } = true;

    /// <summary>
    /// Base URL used to construct verification links. Should include scheme and host (e.g. https://example.com).
    /// </summary>
    public required string VerificationLinkBaseUrl { get; init; }
}
