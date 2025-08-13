using System.Text.Json.Serialization;

namespace Infrastructure.ExternalApis.Espn.Dtos;

/// <summary>
/// Root response DTO for ESPN teams API
/// </summary>
public class EspnTeamsResponse
{
    [JsonPropertyName("sports")]
    public List<EspnSport> Sports { get; set; } = new();
}

/// <summary>
/// Sport section containing leagues
/// </summary>
public class EspnSport
{
    [JsonPropertyName("leagues")]
    public List<EspnLeague> Leagues { get; set; } = new();
}

/// <summary>
/// League section containing teams
/// </summary>
public class EspnLeague
{
    [JsonPropertyName("teams")]
    public List<EspnTeam> Teams { get; set; } = new();
}

/// <summary>
/// Individual team DTO from ESPN API
/// </summary>
public class EspnTeam
{
    [JsonPropertyName("team")]
    public EspnTeamDetails Team { get; set; } = new();
}

/// <summary>
/// Detailed team information from ESPN API
/// </summary>
public class EspnTeamDetails
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;
    
    [JsonPropertyName("abbreviation")]
    public string Abbreviation { get; set; } = string.Empty;
    
    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;
    
    [JsonPropertyName("nickname")]
    public string Nickname { get; set; } = string.Empty;
    
    [JsonPropertyName("color")]
    public string? Color { get; set; }
    
    [JsonPropertyName("alternateColor")]
    public string? AlternateColor { get; set; }
    
    [JsonPropertyName("logos")]
    public List<EspnLogo> Logos { get; set; } = new();
    
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Team logo information from ESPN API
/// </summary>
public class EspnLogo
{
    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;
    
    [JsonPropertyName("alt")]
    public string Alt { get; set; } = string.Empty;
    
    [JsonPropertyName("width")]
    public int Width { get; set; }
    
    [JsonPropertyName("height")]
    public int Height { get; set; }
}