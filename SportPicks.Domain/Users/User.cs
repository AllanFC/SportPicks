namespace Domain.Users;

public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string HashedPassword { get; private set; }
    public string Salt { get; private set; }
    public DateTime CreatedAt { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private User() { } // Required for EF Core
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public User(string username, string email, string hashedPassword, string salt)
    {
        Id = Guid.NewGuid();
        Username = username;
        Email = email;
        HashedPassword = hashedPassword;
        Salt = salt;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdatePassword(string hashedPassword, string salt)
    {
        HashedPassword = hashedPassword;
        Salt = salt;
    }
}
