namespace Application.Options;

public class NflSyncSettings
{
    public const string SectionKey = "NflSync";
    
    public string BaseUrl { get; set; } = "https://site.api.espn.com";
    public int? TargetSeason { get; set; } = null; // Override current season detection
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
    public int RetryDelayMs { get; set; } = 1000;
    public int ScoreboardLimit { get; set; } = 1000;
}