using MeydanCleanApi.Template.Application.Abstractions.Messaging;

namespace MeydanCleanApi.Template.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Command payload for renewing an access token using a valid refresh token.
/// </summary>
/// <param name="RefreshToken">
/// Optional. Browser clients leave this empty because the token is read from the HttpOnly cookie.
/// Native clients that cannot store cookies send it here instead.
/// </param>
public sealed record RefreshTokenCommand(string? RefreshToken = null) : IRequest<BaseResponse<RefreshTokenCommandResponse>>;
