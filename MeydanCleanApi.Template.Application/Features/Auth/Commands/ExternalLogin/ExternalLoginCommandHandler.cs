using MeydanCleanApi.Template.Application.Abstractions.Auth;
using MeydanCleanApi.Template.Application.Abstractions.Messaging;
using MeydanCleanApi.Template.Application.Common.Models.Token;

namespace MeydanCleanApi.Template.Application.Features.Auth.Commands.ExternalLogin;

/// <summary>
/// Command handler for external OAuth authentication via <see cref="IAuthService"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ExternalLoginCommandHandler"/> class.
/// </remarks>
public sealed class ExternalLoginCommandHandler(
    IAuthService authService,
    IRefreshTokenDeliveryService refreshTokenDelivery) : IRequestHandler<ExternalLoginCommand, BaseResponse<ExternalLoginCommandResponse>>
{
    private readonly IAuthService _authService = authService;
    private readonly IRefreshTokenDeliveryService _refreshTokenDelivery = refreshTokenDelivery;

    /// <inheritdoc />
    public async Task<BaseResponse<ExternalLoginCommandResponse>> Handle(ExternalLoginCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 1. Verify the provider token, then find or create the matching local account.
        var tokenModel = await _authService.ExternalLoginAsync(request.AuthProvider, request.IdToken, ct);

        // 2. Hand the refresh token to the client out of band (HttpOnly cookie), never in the body.
        _refreshTokenDelivery.Issue(tokenModel.RefreshToken, tokenModel.RefreshTokenExpiration);

        return BaseResponse<ExternalLoginCommandResponse>.Success(
            new ExternalLoginCommandResponse(AccessTokenModel.From(tokenModel)));
    }
}
