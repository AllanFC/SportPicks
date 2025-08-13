namespace Application.Options;

/// <summary>
/// Configuration settings for NFL data synchronization
/// </summary>
public class NflSyncSettings
{
    /// <summary>
    /// Configuration section key
    /// </summary>
    public const string SectionKey = "NflSync";
    
    /// <summary>
    /// Base URL for ESPN NFL API (domain only, paths are included in endpoints)
    /// </summary>
    public string BaseUrl { get; set; } = "https://site.api.espn.com";
    
    /// <summary>
    /// Target NFL season year (null = auto-detect current season)
    /// </summary>
    public int? TargetSeason { get; set; } = null;
    
    /// <summary>
    /// HTTP client timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Maximum number of retries for failed requests
    /// </summary>
    public int MaxRetries { get; set; } = 3;
    
    /// <summary>
    /// Delay between retries in milliseconds
    /// </summary>
    public int RetryDelayMs { get; set; } = 1000;
    
    /// <summary>
    /// Maximum limit for scoreboard requests
    /// </summary>
    public int ScoreboardLimit { get; set; } = 1000;
    
    /// <summary>
    /// Days to look back from current date when syncing (default: 30 days)
    /// </summary>
    public int DaysBack { get; set; } = 30;
    
    /// <summary>
    /// Days to look forward from current date when syncing (default: 365 days)
    /// Note: This will be automatically capped to reasonable NFL schedule boundaries
    /// </summary>
    public int DaysForward { get; set; } = 365;
    
    /// <summary>
    /// Gets the current NFL season based on current date
    /// NFL seasons span two calendar years (e.g., 2024 season runs Sep 2024 - Feb 2025)
    /// </summary>
    public int CurrentSeason 
    { 
        get 
        {
            if (TargetSeason.HasValue)
                return TargetSeason.Value;
                
            var now = DateTime.Now;
            
            // NFL season typically starts in September
            // If we're in Jan-July, we're in the previous season's year
            // If we're in Aug-Dec, we're in the current year's season
            return now.Month >= 8 ? now.Year : now.Year - 1;
        }
    }
    
    /// <summary>
    /// Gets dynamic start date for data sync based on current date and configuration
    /// </summary>
    public DateTime SyncStartDate => DateTime.Now.AddDays(-DaysBack);
    
    /// <summary>
    /// Gets dynamic end date for data sync based on current date and configuration
    /// This is intelligently capped to avoid requesting data beyond ESPN's available schedule
    /// </summary>
    public DateTime SyncEndDate 
    { 
        get 
        {
            var now = DateTime.Now;
            var requestedEndDate = now.AddDays(DaysForward);
            
            // Cap the end date to a reasonable NFL schedule boundary
            // ESPN typically has data for current season + next season's preseason/regular season
            var maxReasonableEndDate = GetMaxReasonableNflDate(now);
            
            // Return the earlier of the two dates
            return requestedEndDate <= maxReasonableEndDate ? requestedEndDate : maxReasonableEndDate;
        }
    }
    
    /// <summary>
    /// Gets the maximum reasonable date for NFL data requests based on current date
    /// This prevents requesting data beyond ESPN's available schedule
    /// </summary>
    private static DateTime GetMaxReasonableNflDate(DateTime currentDate)
    {
        var currentMonth = currentDate.Month;
        var currentYear = currentDate.Year;
        
        // Determine what NFL seasons are reasonable to request
        DateTime maxDate;
        
        if (currentMonth >= 3 && currentMonth <= 8)
        {
            // March-August: We're in off-season, can request up to end of next season (February following year)
            // Current season ends in February, next season ends in February the year after
            maxDate = new DateTime(currentYear + 2, 2, 28);
        }
        else if (currentMonth >= 9)
        {
            // September-December: We're in current season, can request up to end of next season
            maxDate = new DateTime(currentYear + 2, 2, 28);
        }
        else
        {
            // January-February: We're at end of current season, can request up to end of next season
            maxDate = new DateTime(currentYear + 1, 2, 28);
        }
        
        // Add some buffer but don't go beyond reasonable limits
        // ESPN typically doesn't have data more than ~18 months in advance
        var maxBufferDate = currentDate.AddMonths(18);
        
        return maxDate <= maxBufferDate ? maxDate : maxBufferDate;
    }
}