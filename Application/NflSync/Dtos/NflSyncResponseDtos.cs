namespace Application.NflSync.Dtos;

/// <summary>
/// Response DTO for NFL sync operations
/// </summary>
public class NflSyncResponseDto
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Human-readable message describing the operation result
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Timestamp when the sync was completed (UTC)
    /// </summary>
    public DateTime SyncedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Username of the user who performed the sync
    /// </summary>
    public string? SyncedBy { get; set; }
}

/// <summary>
/// Response DTO for team synchronization operations
/// </summary>
public class TeamSyncResponseDto : NflSyncResponseDto
{
    /// <summary>
    /// Number of teams that were synchronized
    /// </summary>
    public int TeamCount { get; set; }
}

/// <summary>
/// Response DTO for match synchronization operations
/// </summary>
public class MatchSyncResponseDto : NflSyncResponseDto
{
    /// <summary>
    /// Number of matches that were synchronized
    /// </summary>
    public int MatchCount { get; set; }

    /// <summary>
    /// Season year for which matches were synchronized (optional)
    /// </summary>
    public int? Season { get; set; }

    /// <summary>
    /// Start date for date range sync (optional)
    /// </summary>
    public string? StartDate { get; set; }

    /// <summary>
    /// End date for date range sync (optional)
    /// </summary>
    public string? EndDate { get; set; }
}

/// <summary>
/// Response DTO for full synchronization operations
/// </summary>
public class FullSyncResponseDto : NflSyncResponseDto
{
    /// <summary>
    /// Number of teams that were synchronized
    /// </summary>
    public int TeamCount { get; set; }

    /// <summary>
    /// Number of matches that were synchronized
    /// </summary>
    public int MatchCount { get; set; }

    /// <summary>
    /// Indicates this was a high-impact operation requiring special permissions
    /// </summary>
    public bool IsHighImpactOperation { get; set; } = true;
}

/// <summary>
/// Response DTO for error responses
/// </summary>
public class ErrorResponseDto
{
    /// <summary>
    /// Always false for error responses
    /// </summary>
    public bool Success { get; set; } = false;

    /// <summary>
    /// Error message describing what went wrong
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Detailed error information (only included in development)
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Timestamp when the error occurred (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}