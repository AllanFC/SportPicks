using System.Diagnostics.CodeAnalysis;

namespace Domain.Sports;

/// <summary>
/// Each competitor and predicted position in a ranked pick
/// </summary>
public class RankedPickDetail
{
    public Guid Id { get; set; }
    public Guid RankedPickId { get; set; }
    public Guid CompetitorId { get; set; }
    public int PredictedPosition { get; set; } // 1st, 2nd, 3rd, etc.
    
    // Scoring
    public int Points { get; set; } = 0;
    public int? ActualPosition { get; set; } // null = not yet determined
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual RankedPick RankedPick { get; set; } = null!;
    public virtual Competitor Competitor { get; set; } = null!;

    private RankedPickDetail() { } // Required for EF Core

    [SetsRequiredMembers]
    public RankedPickDetail(Guid rankedPickId, Guid competitorId, int predictedPosition)
    {
        Id = Guid.NewGuid();
        RankedPickId = rankedPickId;
        CompetitorId = competitorId;
        PredictedPosition = predictedPosition;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePosition(int predictedPosition)
    {
        PredictedPosition = predictedPosition;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ScoreDetail(int points, int? actualPosition = null)
    {
        Points = points;
        ActualPosition = actualPosition;
        UpdatedAt = DateTime.UtcNow;
    }
}