namespace Application.Common.Interfaces;

/// <summary>
/// Service for managing user roles and administrative operations
/// </summary>
public interface IUserRoleService
{
    /// <summary>
    /// Promotes a user to Admin role
    /// </summary>
    /// <param name="userId">User ID to promote</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    Task<bool> PromoteToAdminAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Promotes a user to Admin role by email
    /// </summary>
    /// <param name="email">Email of user to promote</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    Task<bool> PromoteToAdminAsync(string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Demotes an admin user to regular user role
    /// </summary>
    /// <param name="userId">User ID to demote</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    Task<bool> DemoteToUserAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all admin users
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of admin users</returns>
    Task<List<User>> GetAdminUsersAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if a user is an admin
    /// </summary>
    /// <param name="userId">User ID to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user is admin</returns>
    Task<bool> IsAdminAsync(Guid userId, CancellationToken cancellationToken = default);
}