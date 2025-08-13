namespace SportPicks.API.Authorization;

/// <summary>
/// Authorization policy names and configuration for the application
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>
    /// Policy for administrative operations requiring Admin role
    /// </summary>
    public const string AdminOnly = "AdminOnly";
    
    /// <summary>
    /// Policy for NFL data synchronization operations
    /// </summary>
    public const string NflDataSync = "NflDataSync";
    
    /// <summary>
    /// Policy for high-impact operations that require additional logging
    /// </summary>
    public const string HighImpactOperations = "HighImpactOperations";

    /// <summary>
    /// Configures all authorization policies for the application
    /// </summary>
    /// <param name="options">Authorization options</param>
    public static void ConfigurePolicies(AuthorizationOptions options)
    {
        // Basic admin policy - requires Admin role
        options.AddPolicy(AdminOnly, policy =>
            policy.RequireRole(UserRolesEnum.Admin.ToString()));

        // NFL data sync policy - requires Admin role with additional claims if needed
        options.AddPolicy(NflDataSync, policy =>
            policy.RequireRole(UserRolesEnum.Admin.ToString())
                  .RequireAuthenticatedUser());

        // High impact operations - stricter requirements
        options.AddPolicy(HighImpactOperations, policy =>
            policy.RequireRole(UserRolesEnum.Admin.ToString())
                  .RequireAuthenticatedUser()
                  .RequireAssertion(context =>
                  {
                      // Additional validation logic can be added here
                      // For now, just ensure the user is an admin
                      return context.User.IsInRole(UserRolesEnum.Admin.ToString());
                  }));

        // Optional: Set a fallback policy to require authentication by default
        // Uncomment if you want all endpoints to require authentication by default
        // options.FallbackPolicy = new AuthorizationPolicyBuilder()
        //     .RequireAuthenticatedUser()
        //     .Build();
    }
}