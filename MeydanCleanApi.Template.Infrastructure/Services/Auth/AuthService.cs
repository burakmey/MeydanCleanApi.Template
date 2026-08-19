using MeydanCleanApi.Template.Application.Abstractions.Auth;
using MeydanCleanApi.Template.Application.Abstractions.Repositories;
using MeydanCleanApi.Template.Application.Common.Models.Auth;
using MeydanCleanApi.Template.Application.Common.Models.Token;
using MeydanCleanApi.Template.Domain.Entities.Auths;
using MeydanCleanApi.Template.Domain.Entities.Identity;
using MeydanCleanApi.Template.Domain.Exceptions;
using MeydanCleanApi.Template.Infrastructure.Common.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace MeydanCleanApi.Template.Infrastructure.Services.Auth;

/// <summary>
/// Infrastructure authentication service implementing <see cref="IAuthService"/> on top of ASP.NET Core Identity.
/// Handles password login, refresh token rotation, and external OAuth sign-in with account linking.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Revocation:</strong> logging out clears the stored refresh token straight away, so no new access
/// token can be issued for that session. An access token that was already handed out stays valid until it
/// expires, which is why <c>AccessTokenExpiryMinutes</c> is kept short. For instant revocation you would
/// also have to compare the token's <c>jti</c> against the stored one on every request, which costs a
/// database read per call.
/// </para>
/// </remarks>
public sealed class AuthService(
    UserManager<AppUser> userManager,
    ITokenService tokenService,
    IUserSessionService userSessionService,
    IReadRepository<UserAuthProvider, (Guid UserId, int AuthProviderId)> userAuthProviderReadRepository,
    IWriteRepository<UserAuthProvider, (Guid UserId, int AuthProviderId)> userAuthProviderWriteRepository,
    IUnitOfWork unitOfWork,
    IExternalAuthVerifierResolver verifierResolver,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IUserSessionService _userSessionService = userSessionService;
    private readonly IReadRepository<UserAuthProvider, (Guid UserId, int AuthProviderId)> _userAuthProviderReadRepository = userAuthProviderReadRepository;
    private readonly IWriteRepository<UserAuthProvider, (Guid UserId, int AuthProviderId)> _userAuthProviderWriteRepository = userAuthProviderWriteRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IExternalAuthVerifierResolver _verifierResolver = verifierResolver;
    private readonly ILogger<AuthService> _logger = logger;

    /// <inheritdoc />
    public async Task<TokenModel> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        // 1. Find the account. Every failure below returns the same error on purpose, so an attacker
        //    cannot tell "no such user" apart from "wrong password".
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            _logger.LogWarning(LogEvents.AuthFailed, "Login failed: no account matches the supplied email.");
            throw InvalidCredentialsException.WithCode();
        }

        // 2. Refuse locked-out accounts before checking the password.
        if (await _userManager.IsLockedOutAsync(user))
        {
            _logger.LogWarning(LogEvents.AuthFailed, "Login failed: account {UserId} is locked out.", user.Id);
            throw InvalidCredentialsException.WithCode();
        }

        // 3. Verify the password. On failure record the attempt so repeated guesses trigger a lockout.
        if (!await _userManager.CheckPasswordAsync(user, password))
        {
            await _userManager.AccessFailedAsync(user);
            _logger.LogWarning(LogEvents.AuthFailed, "Login failed: wrong password for account {UserId}.", user.Id);
            throw InvalidCredentialsException.WithCode();
        }

        // 4. A successful login clears the failed-attempt counter.
        await _userManager.ResetAccessFailedCountAsync(user);

        // 5. Issue the token pair and store the session.
        var tokenModel = await IssueTokensAsync(user, AuthProviderType.Local, ct);

        _logger.LogInformation(LogEvents.TokenIssued, "Login succeeded for account {UserId}.", user.Id);
        return tokenModel;
    }

    /// <inheritdoc />
    public async Task<TokenModel> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        // 1. Look the session up by the hash of the presented token. The plain token is never stored.
        //    The lookup also filters out expired sessions, so an old token finds nothing.
        var tokenHash = _tokenService.HashRefreshToken(refreshToken);
        var userId = await _userSessionService.FindUserIdByRefreshTokenHashAsync(tokenHash, ct);

        if (userId is null)
        {
            _logger.LogWarning(LogEvents.AuthFailed, "Refresh failed: no active session matches the presented token.");
            throw UnauthorizedException.WithCode(ErrorCodes.SessionExpired);
        }

        var user = await _userManager.FindByIdAsync(userId.Value.ToString())
            ?? throw UnauthorizedException.WithCode(ErrorCodes.SessionExpired);

        // 2. Rotate. Issuing a new pair overwrites the stored hash, so a stolen copy of the old
        //    refresh token stops working the moment it is used once.
        var tokenModel = await IssueTokensAsync(user, (AuthProviderType)user.ActiveAuthProviderId, ct);

        _logger.LogInformation(LogEvents.TokenIssued, "Refresh succeeded for account {UserId}.", user.Id);
        return tokenModel;
    }

    /// <inheritdoc />
    public async Task<TokenModel> ExternalLoginAsync(AuthProviderType authProvider, string idToken, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idToken);

        // 1. Pick the verifier registered for this provider. The resolver rejects Local and any
        //    provider that has no verifier registered.
        var verifier = _verifierResolver.Resolve(authProvider);

        // 2. Verify the token against the provider's published signing keys.
        var externalUser = await verifier.VerifyIdTokenAsync(idToken, ct);

        // 3. Refuse unverified emails. Without this check somebody could register a provider account
        //    using an email they do not own and then take over the matching local account in step 4.
        if (!externalUser.EmailVerified)
        {
            _logger.LogWarning(LogEvents.AuthFailed, "External login rejected: {Provider} reported the email as unverified.", authProvider);
            throw ConflictException.WithCode(ErrorCodes.ExternalAuthMissingClaims);
        }

        // 5. Find or create the account, then make sure the provider link exists.
        var user = await ResolveExternalUserAsync(externalUser, authProvider, ct);

        var tokenModel = await IssueTokensAsync(user, authProvider, ct);

        _logger.LogInformation(
            LogEvents.ExternalAuthVerified, "External login succeeded for account {UserId} via {Provider}.", user.Id, authProvider);

        return tokenModel;
    }

    /// <summary>
    /// Finds the account linked to an external identity, linking or creating one when needed.
    /// </summary>
    private async Task<AppUser> ResolveExternalUserAsync(
        ExternalUserModel externalUser, AuthProviderType authProvider, CancellationToken ct)
    {
        var providerId = (int)authProvider;

        // Already linked. The provider's subject id is the stable identifier here, not the email,
        // because a user can change the email on their provider account.
        var existingLink = await _userAuthProviderReadRepository.FirstOrDefaultAsync(
            link => link.AuthProviderId == providerId && link.ProviderKey == externalUser.Subject, ct: ct);

        if (existingLink is not null)
        {
            return await _userManager.FindByIdAsync(existingLink.UserId.ToString())
                ?? throw InvalidCredentialsException.WithCode();
        }

        // Not linked yet: attach to the account with the same email, or create a new one.
        var user = await _userManager.FindByEmailAsync(externalUser.Email);
        if (user is null)
        {
            user = new AppUser
            {
                UserName = externalUser.Email,
                Email = externalUser.Email,
                EmailConfirmed = true,
                ActiveAuthProviderId = providerId
            };

            // Created without a password, so this account can only sign in through the provider.
            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                _logger.LogError(
                    LogEvents.AuthFailed,
                    "Could not create an account for an external login. {Errors}",
                    string.Join("; ", createResult.Errors.Select(e => e.Code)));

                throw ConflictException.WithCode(ErrorCodes.EmailInUse);
            }

            await _userManager.AddToRoleAsync(user, RoleConstants.Customer);
        }

        await _userAuthProviderWriteRepository.AddAsync(new UserAuthProvider
        {
            UserId = user.Id,
            AuthProviderId = providerId,
            ProviderKey = externalUser.Subject
        }, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        return user;
    }

    /// <summary>
    /// Creates a token pair and saves the resulting session.
    /// </summary>
    private async Task<TokenModel> IssueTokensAsync(AppUser user, AuthProviderType authProvider, CancellationToken ct)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var tokenModel = _tokenService.CreateTokens(user.Id, user.Email!, roles);

        if (user.ActiveAuthProviderId != (int)authProvider)
        {
            user.ActiveAuthProviderId = (int)authProvider;
            await _userManager.UpdateAsync(user);
        }

        await _userSessionService.StoreAsync(
            user.Id,
            _tokenService.HashRefreshToken(tokenModel.RefreshToken),
            tokenModel.RefreshTokenExpiration,
            tokenModel.Jti,
            ct);

        return tokenModel;
    }
}
