namespace Application.Common.Interfaces;

public interface IUserRepository
{
    Task<bool> IsUsernameTakenAsync(string username);
    Task<bool> IsEmailTakenAsync(string email);
    Task AddUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task <User?> GetUserByIdAsync(Guid id);
    Task <User?> GetUserByRefreshTokenAsync(string refreshToken);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
}
