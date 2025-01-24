namespace Domain.Users;

public class User
{
    public Guid Id { get; set; }             // Primary key
    public string Username { get; set; }    // For local login
    public string Email { get; set; }
    public string PasswordHash { get; set; } // Hashed password for local login
    public string Salt { get; set; }        // Salt for hashing

    public string Provider { get; set; } = "Local";  // "Local", "Google", "Facebook"
    public string? ProviderId { get; set; }  // Unique provider user ID

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private User() { } // Required for EF Core
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public User(string username, string email, string passwordHash, string salt)
    {
        Id = Guid.NewGuid();
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Salt = salt;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdatePassword(string passwordHash, string salt)
    {
        PasswordHash = passwordHash;
        Salt = salt;
    }
}
