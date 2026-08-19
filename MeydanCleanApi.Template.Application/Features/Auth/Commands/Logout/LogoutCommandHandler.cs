using MeydanCleanApi.Template.Application.Abstractions.Auth;
using MeydanCleanApi.Template.Application.Abstractions.Messaging;

namespace MeydanCleanApi.Template.Application.Features.Auth.Commands.Logout;

/// <summary>
/// Command handler for user logout operation.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="LogoutCommandHandler"/> class.
/// </remarks>
public sealed class LogoutCommandHandler(
    ICurrentUserService currentUserService,
    IUserSessionService userSessionService,
    IRefreshTokenDeliveryService refreshTokenDelivery) : IRequestHandler<LogoutCommand, BaseResponse<LogoutCommandResponse>>
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IUserSessionService _userSessionService = userSessionService;
    private readonly IRefreshTokenDeliveryService _refreshTokenDelivery = refreshTokenDelivery;

    /// <inheritdoc />
    public async Task<BaseResponse<LogoutCommandResponse>> Handle(LogoutCommand request, CancellationToken ct)
    {
        // 1. Identify the caller. The endpoint requires authentication, so this is normally set.
        var userId = _currentUserService.UserId
            ?? throw UnauthorizedException.WithCode();

        // 2. Delete the stored refresh token so it can no longer be exchanged for a new access token.
        //    Clearing the browser cookie alone is not enough: anyone holding a copy of the token
        //    could keep using it.
        await _userSessionService.RevokeAllUserSessionsAsync(userId, ct);

        // 3. Remove the client's copy as well.
        _refreshTokenDelivery.Revoke();

        return BaseResponse<LogoutCommandResponse>.Success(new LogoutCommandResponse(true));
    }
}
