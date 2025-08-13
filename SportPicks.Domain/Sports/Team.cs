using System.Diagnostics.CodeAnalysis;

namespace Domain.Sports;

/// <summary>
/// Represents an NFL team entity
/// </summary>
public class Team
{
    /// <summary>
    /// Unique identifier for the team from ESPN API
    /// </summary>
    public required string EspnId { get; set; }
    
    /// <summary>
    /// Display name of the team (e.g., "New England Patriots")
    /// </summary>
    public required string DisplayName { get; set; }
    
    /// <summary>
    /// Team abbreviation (e.g., "NE")
    /// </summary>
    public required string Abbreviation { get; set; }
    
    /// <summary>
    /// Team location (e.g., "New England")
    /// </summary>
    public required string Location { get; set; }
    
    /// <summary>
    /// Team nickname (e.g., "Patriots")
    /// </summary>
    public required string Nickname { get; set; }
    
    /// <summary>
    /// Primary team logo URL
    /// </summary>
    public string? LogoUrl { get; set; }
    
    /// <summary>
    /// Team's primary color
    /// </summary>
    public string? Color { get; set; }
    
    /// <summary>
    /// Team's alternate color
    /// </summary>
    public string? AlternateColor { get; set; }
    
    /// <summary>
    /// Indicates if the team is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// When the team record was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When the team record was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    private Team() { } // Required for EF Core

    [SetsRequiredMembers]
    public Team(string espnId, string displayName, string abbreviation, string location, string nickname)
    {
        EspnId = espnId;
        DisplayName = displayName;
        Abbreviation = abbreviation;
        Location = location;
        Nickname = nickname;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates team information
    /// </summary>
    public void UpdateTeamInfo(string displayName, string abbreviation, string location, string nickname, string? logoUrl = null, string? color = null, string? alternateColor = null)
    {
        DisplayName = displayName;
        Abbreviation = abbreviation;
        Location = location;
        Nickname = nickname;
        LogoUrl = logoUrl;
        Color = color;
        AlternateColor = alternateColor;
        UpdatedAt = DateTime.UtcNow;
    }
}