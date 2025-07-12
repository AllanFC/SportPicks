using System.Diagnostics.CodeAnalysis;

namespace Domain.Users;

public class User
{
    public Guid Id { get; set; }             // Primary key
    public required string Username { get; set; }    // For local login
    public required string Email { get; set; }
    public string UserRole { get; set; } = UserRolesEnum.User.ToString();
    public required string PasswordHash { get; set; } // Hashed password for local login
    public required string Salt { get; set; }        // Salt for hashing

    public string? RefreshToken { get; set; } // JWT refresh token
    public DateTime RefreshTokenExpiry { get; set; } = DateTime.UtcNow; // Expiry date of refresh token

    public string Provider { get; set; } = "Local";  // "Local", "Google", "Facebook"
    public string? ProviderId { get; set; }  // Unique provider user ID

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    private User() { } // Required for EF Core

    [SetsRequiredMembers]
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
