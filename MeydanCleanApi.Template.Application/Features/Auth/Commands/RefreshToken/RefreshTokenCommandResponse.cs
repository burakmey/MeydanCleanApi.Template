using MeydanCleanApi.Template.Application.Common.Models.Token;

namespace MeydanCleanApi.Template.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Response payload returned after a successful RefreshToken operation.
/// </summary>
/// <param name="Token">Access token and its expiry. The refresh token is returned as an HttpOnly cookie instead.</param>
public sealed record RefreshTokenCommandResponse(AccessTokenModel Token);
