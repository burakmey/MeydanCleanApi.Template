using MeydanCleanApi.Template.Application.Common.Models.Token;

namespace MeydanCleanApi.Template.Application.Features.Auth.Commands.Login;

/// <summary>
/// Response payload returned after a successful Login operation.
/// </summary>
/// <param name="Token">Access token and its expiry. The refresh token is returned as an HttpOnly cookie instead.</param>
public sealed record LoginCommandResponse(AccessTokenModel Token);
