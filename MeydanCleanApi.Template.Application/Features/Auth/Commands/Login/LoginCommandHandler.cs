using MeydanCleanApi.Template.Application.Abstractions.Auth;
using MeydanCleanApi.Template.Application.Abstractions.Messaging;
using MeydanCleanApi.Template.Application.Common.Models.Token;

namespace MeydanCleanApi.Template.Application.Features.Auth.Commands.Login;

/// <summary>
/// Command handler for authenticating credentials via <see cref="IAuthService"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="LoginCommandHandler"/> class.
/// </remarks>
public sealed class LoginCommandHandler(
    IAuthService authService,
    IRefreshTokenDeliveryService refreshTokenDelivery) : IRequestHandler<LoginCommand, BaseResponse<LoginCommandResponse>>
{
    private readonly IAuthService _authService = authService;
    private readonly IRefreshTokenDeliveryService _refreshTokenDelivery = refreshTokenDelivery;

    /// <inheritdoc />
    public async Task<BaseResponse<LoginCommandResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 1. Check the credentials and create a new session.
        var tokenModel = await _authService.LoginAsync(request.Email, request.Password, ct);

        // 2. Hand the refresh token to the client out of band (HttpOnly cookie), never in the body.
        _refreshTokenDelivery.Issue(tokenModel.RefreshToken, tokenModel.RefreshTokenExpiration);

        // 3. Return only the access token to the caller.
        return BaseResponse<LoginCommandResponse>.Success(
            new LoginCommandResponse(AccessTokenModel.From(tokenModel)));
    }
}
