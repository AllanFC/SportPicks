namespace Application.Common.Interfaces;

public interface IUserService
{
    public Task<User?> LoginAsync(string email, string password);
    public Task<Guid> RegisterUserAsync(string username, string email, string password);
    public Task UpdateUserPasswordAsync(string email, string oldPassword, string newPassword);
}
