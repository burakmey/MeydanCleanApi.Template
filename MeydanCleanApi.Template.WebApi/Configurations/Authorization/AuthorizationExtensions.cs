namespace MeydanCleanApi.Template.WebApi.Configurations.Authorization;

/// <summary>
/// Extension methods for registering application authorization policies (Role-Based and Policy-Based).
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Registers role-based and policy-based authorization policies (SuperAdmin, Admin, Customer).
    /// </summary>
    public static IServiceCollection AddApplicationAuthorization(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddAuthorization(options =>
        {
            // Endpoints require a signed-in user unless they opt out with [AllowAnonymous].
            // Without this, forgetting [Authorize] on a new controller silently makes it public.
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            // Policy restricting access strictly to SuperAdmin users
            options.AddPolicy(PolicyConstants.SuperAdmin, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireRole(RoleConstants.SuperAdmin));

            // Policy granting access to Admin and SuperAdmin users.
            // SuperAdmin is included deliberately so highest privilege users satisfy ordinary admin endpoints.
            options.AddPolicy(PolicyConstants.Admin, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireRole(RoleConstants.SuperAdmin, RoleConstants.Admin));

            // Policy restricting access to Customer users
            options.AddPolicy(PolicyConstants.Customer, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireRole(RoleConstants.Customer));
        });

        return services;
    }
}
