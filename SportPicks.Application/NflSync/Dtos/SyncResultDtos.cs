namespace Application.NflSync.Dtos;

/// <summary>
/// Teams sync result DTO
/// </summary>
public class TeamsSyncResultDto
{
    public int TeamCount { get; set; }
    public List<TeamDto> Teams { get; set; } = new();
}

/// <summary>
/// Team DTO for sync operations
/// </summary>
public class TeamDto
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
}

/// <summary>
/// Matches sync result DTO
/// </summary>
public class MatchesSyncResultDto
{
    public int MatchCount { get; set; }
    public List<MatchDto> Matches { get; set; } = new();
}

/// <summary>
/// Match DTO for sync operations
/// </summary>
public class MatchDto
{
    public required string EspnId { get; set; }
    public required string Name { get; set; }
    public DateTime MatchDate { get; set; }
    public int Season { get; set; }
    public int Week { get; set; }
    public required string HomeTeamEspnId { get; set; }
    public required string AwayTeamEspnId { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public required string Status { get; set; }
    public bool IsCompleted { get; set; }
    public string? Venue { get; set; }
}