namespace Domain.Common;

public abstract class BaseEntity
{
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public void UpdateTimestamp() => UpdatedAt = DateTime.UtcNow;
}
