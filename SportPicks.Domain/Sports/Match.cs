using System.Diagnostics.CodeAnalysis;

namespace Domain.Sports;

/// <summary>
/// Represents an NFL match/game entity
/// </summary>
public class Match
{
    /// <summary>
    /// Unique identifier for the match from ESPN API
    /// </summary>
    public required string EspnId { get; set; }
    
    /// <summary>
    /// Display name of the match (e.g., "Patriots at Bills")
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// Date and time of the match
    /// </summary>
    public DateTime MatchDate { get; set; }
    
    /// <summary>
    /// Season year (e.g., 2024)
    /// </summary>
    public int Season { get; set; }
    
    /// <summary>
    /// Season type: 1=Preseason, 2=Regular, 3=Postseason
    /// </summary>
    public int SeasonType { get; set; }
    
    /// <summary>
    /// Season type name (preseason, regular, postseason)
    /// </summary>
    public string SeasonTypeSlug { get; set; } = string.Empty;
    
    /// <summary>
    /// Week number in the season
    /// </summary>
    public int Week { get; set; }
    
    /// <summary>
    /// ESPN ID of the home team
    /// </summary>
    public required string HomeTeamEspnId { get; set; }
    
    /// <summary>
    /// ESPN ID of the away team
    /// </summary>
    public required string AwayTeamEspnId { get; set; }
    
    /// <summary>
    /// Home team's score (null if game hasn't started or is in progress)
    /// </summary>
    public int? HomeScore { get; set; }
    
    /// <summary>
    /// Away team's score (null if game hasn't started or is in progress)
    /// </summary>
    public int? AwayScore { get; set; }
    
    /// <summary>
    /// Current status of the match
    /// </summary>
    public required string Status { get; set; }
    
    /// <summary>
    /// Indicates if the match is completed
    /// </summary>
    public bool IsCompleted { get; set; }
    
    /// <summary>
    /// Venue name where the match is played
    /// </summary>
    public string? Venue { get; set; }
    
    /// <summary>
    /// When the match record was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When the match record was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    private Match() { } // Required for EF Core

    [SetsRequiredMembers]
    public Match(string espnId, string name, DateTime matchDate, int season, int seasonType, string seasonTypeSlug, 
                 int week, string homeTeamEspnId, string awayTeamEspnId, string status, bool isCompleted)
    {
        EspnId = espnId;
        Name = name;
        MatchDate = matchDate;
        Season = season;
        SeasonType = seasonType;
        SeasonTypeSlug = seasonTypeSlug;
        Week = week;
        HomeTeamEspnId = homeTeamEspnId;
        AwayTeamEspnId = awayTeamEspnId;
        Status = status;
        IsCompleted = isCompleted;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates match information and scores
    /// </summary>
    public void UpdateMatch(string name, DateTime matchDate, string status, bool isCompleted, 
                           int? homeScore = null, int? awayScore = null, string? venue = null)
    {
        Name = name;
        MatchDate = matchDate;
        Status = status;
        IsCompleted = isCompleted;
        HomeScore = homeScore;
        AwayScore = awayScore;
        Venue = venue;
        UpdatedAt = DateTime.UtcNow;
    }
}