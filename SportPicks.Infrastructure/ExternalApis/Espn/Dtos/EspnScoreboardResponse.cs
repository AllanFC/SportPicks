using System.Text.Json.Serialization;

namespace Infrastructure.ExternalApis.Espn.Dtos;

/// <summary>
/// Root response DTO for ESPN scoreboard API
/// </summary>
public class EspnScoreboardResponse
{
    [JsonPropertyName("events")]
    public List<EspnEvent> Events { get; set; } = new();
    
    [JsonPropertyName("season")]
    public EspnSeason? Season { get; set; }
    
    [JsonPropertyName("week")]
    public EspnWeek? Week { get; set; }
}

/// <summary>
/// Individual event/match from ESPN API
/// </summary>
public class EspnEvent
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;
    
    [JsonPropertyName("status")]
    public EspnStatus Status { get; set; } = new();
    
    [JsonPropertyName("competitions")]
    public List<EspnCompetition> Competitions { get; set; } = new();
    
    [JsonPropertyName("week")]
    public EspnWeek? Week { get; set; }
    
    [JsonPropertyName("season")]
    public EspnSeason? Season { get; set; }
}

/// <summary>
/// Match status information
/// </summary>
public class EspnStatus
{
    [JsonPropertyName("type")]
    public EspnStatusType Type { get; set; } = new();
    
    [JsonPropertyName("displayClock")]
    public string? DisplayClock { get; set; }
    
    [JsonPropertyName("period")]
    public int Period { get; set; }
}

/// <summary>
/// Status type details
/// </summary>
public class EspnStatusType
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;
    
    [JsonPropertyName("completed")]
    public bool Completed { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("detail")]
    public string Detail { get; set; } = string.Empty;
    
    [JsonPropertyName("shortDetail")]
    public string ShortDetail { get; set; } = string.Empty;
}

/// <summary>
/// Competition/match details
/// </summary>
public class EspnCompetition
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("competitors")]
    public List<EspnCompetitor> Competitors { get; set; } = new();
    
    [JsonPropertyName("venue")]
    public EspnVenue? Venue { get; set; }
}

/// <summary>
/// Team competitor in a match
/// </summary>
public class EspnCompetitor
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("homeAway")]
    public string HomeAway { get; set; } = string.Empty;
    
    [JsonPropertyName("team")]
    public EspnCompetitorTeam Team { get; set; } = new();
    
    [JsonPropertyName("score")]
    public string? Score { get; set; }
    
    [JsonPropertyName("winner")]
    public bool? Winner { get; set; }
}

/// <summary>
/// Team information within a competitor
/// </summary>
public class EspnCompetitorTeam
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;
    
    [JsonPropertyName("abbreviation")]
    public string Abbreviation { get; set; } = string.Empty;
}

/// <summary>
/// Venue information
/// </summary>
public class EspnVenue
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = string.Empty;
    
    [JsonPropertyName("address")]
    public EspnAddress? Address { get; set; }
}

/// <summary>
/// Venue address information
/// </summary>
public class EspnAddress
{
    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;
    
    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;
}

/// <summary>
/// Season information
/// </summary>
public class EspnSeason
{
    [JsonPropertyName("year")]
    public int Year { get; set; }
    
    [JsonPropertyName("type")]
    public int Type { get; set; }
    
    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;
}

/// <summary>
/// Week information
/// </summary>
public class EspnWeek
{
    [JsonPropertyName("number")]
    public int Number { get; set; }
}