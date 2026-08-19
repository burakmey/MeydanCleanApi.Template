using MeydanCleanApi.Template.Application.Common.Models.Token;

namespace MeydanCleanApi.Template.Application.Features.Auth.Commands.ExternalLogin;

/// <summary>
/// Response payload returned after a successful ExternalLogin operation.
/// </summary>
/// <param name="Token">Access token and its expiry. The refresh token is returned as an HttpOnly cookie instead.</param>
public sealed record ExternalLoginCommandResponse(AccessTokenModel Token);
