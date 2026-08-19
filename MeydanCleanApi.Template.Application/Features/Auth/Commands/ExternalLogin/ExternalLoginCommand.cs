using MeydanCleanApi.Template.Application.Abstractions.Messaging;
using MeydanCleanApi.Template.Domain.Enums;

namespace MeydanCleanApi.Template.Application.Features.Auth.Commands.ExternalLogin;

/// <summary>
/// Command payload for authenticating via external OAuth providers (e.g. Google).
/// </summary>
/// <param name="AuthProvider">External authentication provider type (1=Google).</param>
/// <param name="IdToken">OAuth ID token payload issued by the provider.</param>
public sealed record ExternalLoginCommand(AuthProviderType AuthProvider, string IdToken) : IRequest<BaseResponse<ExternalLoginCommandResponse>>;
