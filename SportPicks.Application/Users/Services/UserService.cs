using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Application.Users.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UserService> _logger;

    public UserService(ILogger<UserService> logger, IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _logger = logger;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> RegisterUserAsync(string username, string email, string password)
    {
        if (await _userRepository.IsUsernameTakenAsync(username))
            throw new InvalidOperationException("Username is already taken.");

        if (await _userRepository.IsEmailTakenAsync(email))
        {
            throw new InvalidOperationException("Email is already taken.");
        }

        var (hashedPassword, salt) = _passwordHasher.HashPassword(password);

        var user = new User(username, email, hashedPassword, salt);
        await _userRepository.AddUserAsync(user);

        return user.Id;
    }

    public async Task<User?> LoginAsync(string emailOrUsername, string password)
    {
        var regex = new Regex("^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$");
        var isEmail = regex.IsMatch(emailOrUsername);
        var user = isEmail ? await _userRepository.GetUserByEmailAsync(emailOrUsername)
                            : await _userRepository.GetUserByUsernameAsync(emailOrUsername);
        if (user == null) return null;

        // Verify hashed password
        var isValid = _passwordHasher.VerifyPassword(password, user.PasswordHash, user.Salt);

        _logger.LogInformation("User login attempt: {Email}, {IsValid}", emailOrUsername, isValid);

        return isValid ? user : null;
    }

    public async Task UpdateUserPasswordAsync(string email, string oldPassword, string newPassword)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);

        if (user == null) throw new InvalidOperationException("User not found.");

        var (hashedPassword, salt) = _passwordHasher.HashPassword(newPassword);

        if (!_passwordHasher.VerifyPassword(oldPassword, user.PasswordHash, user.Salt))
            throw new InvalidOperationException("Old password is incorrect.");

        user.UpdatePassword(hashedPassword, salt);
        await _userRepository.UpdateUserAsync(user);

        _logger.LogInformation("User password updated: {UserId}", user.Id);
    }
}
