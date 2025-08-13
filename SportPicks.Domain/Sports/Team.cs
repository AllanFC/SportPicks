using System.Diagnostics.CodeAnalysis;

namespace Domain.Sports;

public class Team
{
    public required string EspnId { get; set; }
    public required string DisplayName { get; set; }
    public required string Abbreviation { get; set; }
    public required string Location { get; set; }
    public required string Nickname { get; set; }
    public string? LogoUrl { get; set; }
    public string? Color { get; set; }
    public string? AlternateColor { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
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

    public void UpdateTeamInfo(string displayName, string abbreviation, string location, string nickname, 
                               string? logoUrl = null, string? color = null, string? alternateColor = null)
    {
        DisplayName = displayName;
        Abbreviation = abbreviation;
        Location = location;
        Nickname = nickname;
        LogoUrl = logoUrl;
        Color = color;
        AlternateColor = alternateColor;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}