namespace PutZige.API.Configuration;

public class LoggingSettings
{
    public const string SectionName = "LoggingSettings";
    
    public string ApplicationName { get; set; } = "PutZige";
    public string Environment { get; set; } = "Unknown";
    public bool EnableSeq { get; set; } = false;
    public string SeqUrl { get; set; } = "http://localhost:5341";
}
