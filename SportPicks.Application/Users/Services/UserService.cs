namespace Application.Users.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> RegisterUserAsync(string username, string email, string password)
    {
        if (await _userRepository.IsUsernameTakenAsync(username))
            throw new InvalidOperationException("Username is already taken.");

        if (await _userRepository.IsEmailTakenAsync(email))
            throw new InvalidOperationException("Email is already taken.");

        var (hashedPassword, salt) = _passwordHasher.HashPassword(password);

        var user = new User(username, email, hashedPassword, salt);
        await _userRepository.AddUserAsync(user);

        return user.Id;
    }

    public async Task<User?> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);
        if (user == null) return null;

        // Verify hashed password
        var isValid = _passwordHasher.VerifyPassword(password, user.PasswordHash, user.Salt);
        return isValid ? user : null;
    }
}
