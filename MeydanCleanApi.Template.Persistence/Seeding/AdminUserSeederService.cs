using MeydanCleanApi.Template.Domain.Constants;
using MeydanCleanApi.Template.Domain.Enums;
using MeydanCleanApi.Template.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MeydanCleanApi.Template.Persistence.Seeding;

/// <summary>
/// Seeds default system roles and the initial bootstrap administrator account if no users exist in the database.
/// </summary>
public static class AdminUserSeederService
{
    private const string EmailKey = "SeedData:SuperAdminEmail";
    private const string PasswordKey = "SeedData:SuperAdminPassword";

    /// <summary>
    /// Seeds default system roles (SuperAdmin, Admin, Customer) and creates a single bootstrap SuperAdmin account
    /// if the database contains no users.
    /// </summary>
    /// <param name="roleManager">Identity role manager.</param>
    /// <param name="userManager">Identity user manager.</param>
    /// <param name="context">Application database context for UserAuthProvider entry creation.</param>
    /// <param name="configuration">Configuration instance reading SeedData settings or environment variables.</param>
    /// <param name="logger">Optional logger instance for logging operational info and warnings.</param>
    /// <param name="ct">Cancellation token.</param>
    public static async Task SeedAsync(
        RoleManager<AppRole> roleManager,
        UserManager<AppUser> userManager,
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger? logger = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(roleManager);
        ArgumentNullException.ThrowIfNull(userManager);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(configuration);

        // 1. Seed System Roles
        string[] roles = [RoleConstants.SuperAdmin, RoleConstants.Admin, RoleConstants.Customer];
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new AppRole { Name = roleName, NormalizedName = roleName.ToUpperInvariant() });
            }
        }

        // 2. Empty-Table Guard: On a populated database, administrator seeding is a no-op to guarantee only 1 SuperAdmin bootstrap account exists.
        if (await userManager.Users.AnyAsync(ct))
        {
            logger?.LogInformation("Admin seeding skipped: the database already contains at least one user.");
            return;
        }

        var email = configuration[EmailKey];
        var password = configuration[PasswordKey];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger?.LogInformation("Admin seeding skipped: {EmailKey} / {PasswordKey} are not configured in appsettings or environment variables.", EmailKey, PasswordKey);
            return;
        }

        // 3. Create Bootstrap SuperAdmin Account
        var superAdmin = new AppUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            ActiveAuthProviderId = (int)AuthProviderType.Local
        };

        var createResult = await userManager.CreateAsync(superAdmin, password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
            logger?.LogError("Creating the bootstrap administrator {Email} failed. {Errors}", email, errors);
            return;
        }

        var roleResult = await userManager.AddToRoleAsync(superAdmin, RoleConstants.SuperAdmin);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join("; ", roleResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
            logger?.LogError("Assigning {Role} role to {Email} failed; account created without role. {Errors}", RoleConstants.SuperAdmin, email, errors);
            return;
        }

        // 4. Record Local Auth Provider entry for the SuperAdmin
        context.UserAuthProviders.Add(new UserAuthProvider
        {
            UserId = superAdmin.Id,
            AuthProviderId = (int)AuthProviderType.Local,
            ProviderKey = superAdmin.Id.ToString()
        });

        await context.SaveChangesAsync(ct);

        logger?.LogWarning("Bootstrap administrator {Email} created with {Role} role.", email, RoleConstants.SuperAdmin);
    }
}
