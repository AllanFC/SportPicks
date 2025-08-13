using Microsoft.Extensions.Logging;

namespace Application.Users.Services;

/// <summary>
/// Service for managing user roles and administrative operations
/// </summary>
public class UserRoleService : IUserRoleService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserRoleService> _logger;

    public UserRoleService(IUserRepository userRepository, ILogger<UserRoleService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> PromoteToAdminAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Failed to promote user to admin - User not found: {UserId}", userId);
                return false;
            }

            if (user.UserRole == UserRolesEnum.Admin.ToString())
            {
                _logger.LogInformation("User is already an admin: {Username} ({UserId})", user.Username, userId);
                return true;
            }

            user.UserRole = UserRolesEnum.Admin.ToString();
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateUserAsync(user);

            _logger.LogWarning("User promoted to admin: {Username} ({UserId})", user.Username, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to promote user to admin: {UserId}", userId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> PromoteToAdminAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Failed to promote user to admin - User not found: {Email}", email);
                return false;
            }

            return await PromoteToAdminAsync(user.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to promote user to admin by email: {Email}", email);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DemoteToUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Failed to demote user - User not found: {UserId}", userId);
                return false;
            }

            if (user.UserRole == UserRolesEnum.User.ToString())
            {
                _logger.LogInformation("User is already a regular user: {Username} ({UserId})", user.Username, userId);
                return true;
            }

            user.UserRole = UserRolesEnum.User.ToString();
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateUserAsync(user);

            _logger.LogWarning("User demoted to regular user: {Username} ({UserId})", user.Username, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to demote user: {UserId}", userId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<List<User>> GetAdminUsersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Note: This assumes the repository has a method to filter by role
            // You might need to add this method to IUserRepository if it doesn't exist
            var allUsers = await _userRepository.GetAllUsersAsync(); // You might need to implement this
            return allUsers.Where(u => u.UserRole == UserRolesEnum.Admin.ToString()).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get admin users");
            return new List<User>();
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsAdminAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            return user?.UserRole == UserRolesEnum.Admin.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check admin status for user: {UserId}", userId);
            return false;
        }
    }
}