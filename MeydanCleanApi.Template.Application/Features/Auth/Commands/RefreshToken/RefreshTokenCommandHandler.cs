using MeydanCleanApi.Template.Application.Abstractions.Auth;
using MeydanCleanApi.Template.Application.Abstractions.Messaging;
using MeydanCleanApi.Template.Application.Common.Models.Token;

namespace MeydanCleanApi.Template.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Command handler for renewing tokens via <see cref="IAuthService"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RefreshTokenCommandHandler"/> class.
/// </remarks>
public sealed class RefreshTokenCommandHandler(
    IAuthService authService,
    IRefreshTokenDeliveryService refreshTokenDelivery) : IRequestHandler<RefreshTokenCommand, BaseResponse<RefreshTokenCommandResponse>>
{
    private readonly IAuthService _authService = authService;
    private readonly IRefreshTokenDeliveryService _refreshTokenDelivery = refreshTokenDelivery;

    /// <inheritdoc />
    public async Task<BaseResponse<RefreshTokenCommandResponse>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 1. Prefer the token the client already holds in its cookie. The command value is a fallback
        //    for clients that cannot use cookies, such as native mobile apps.
        var refreshToken = _refreshTokenDelivery.Read() ?? request.RefreshToken;

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw UnauthorizedException.WithCode();
        }

        // 2. Validate the old token and issue a fresh pair. The old one stops working here.
        var tokenModel = await _authService.RefreshTokenAsync(refreshToken, ct);

        // 3. Replace the cookie with the rotated token.
        _refreshTokenDelivery.Issue(tokenModel.RefreshToken, tokenModel.RefreshTokenExpiration);

        return BaseResponse<RefreshTokenCommandResponse>.Success(
            new RefreshTokenCommandResponse(AccessTokenModel.From(tokenModel)));
    }
}
