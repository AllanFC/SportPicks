using System.Text.Json.Serialization;

namespace Infrastructure.ExternalApis.Espn.Dtos;

/// <summary>
/// ESPN Core API season response
/// </summary>
public class EspnCoreSeasonResponse
{
    [JsonPropertyName("year")]
    public int Year { get; set; }
    
    [JsonPropertyName("startDate")]
    public string StartDate { get; set; } = string.Empty;
    
    [JsonPropertyName("endDate")]
    public string EndDate { get; set; } = string.Empty;
    
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;
    
    [JsonPropertyName("types")]
    public EspnCoreSeasonTypes Types { get; set; } = new();
}

/// <summary>
/// Season types collection from ESPN Core API
/// </summary>
public class EspnCoreSeasonTypes
{
    [JsonPropertyName("count")]
    public int Count { get; set; }
    
    [JsonPropertyName("items")]
    public List<EspnCoreSeasonType> Items { get; set; } = new();
}

/// <summary>
/// Individual season type from ESPN Core API
/// </summary>
public class EspnCoreSeasonType
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public int Type { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("abbreviation")]
    public string Abbreviation { get; set; } = string.Empty;
    
    [JsonPropertyName("year")]
    public int Year { get; set; }
    
    [JsonPropertyName("startDate")]
    public string StartDate { get; set; } = string.Empty;
    
    [JsonPropertyName("endDate")]
    public string EndDate { get; set; } = string.Empty;
    
    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;
    
    [JsonPropertyName("week")]
    public EspnCoreWeek? Week { get; set; }
}

/// <summary>
/// Current week information from ESPN Core API
/// </summary>
public class EspnCoreWeek
{
    [JsonPropertyName("number")]
    public int Number { get; set; }
    
    [JsonPropertyName("startDate")]
    public string StartDate { get; set; } = string.Empty;
    
    [JsonPropertyName("endDate")]
    public string EndDate { get; set; } = string.Empty;
    
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}