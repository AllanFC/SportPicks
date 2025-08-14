using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SportPicks.API.Models;

/// <summary>
/// Standard error response model for API endpoints
/// </summary>
public sealed class ErrorResponse
{
    /// <summary>
    /// Human-readable error message
    /// </summary>
    [Required]
    [JsonPropertyName("message")]
    public required string Message { get; set; }
    
    /// <summary>
    /// When the error occurred
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Detailed error information (only included in development)
    /// </summary>
    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; set; }
}