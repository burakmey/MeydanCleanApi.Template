using MeydanCleanApi.Template.Application.Abstractions.Messaging;

namespace MeydanCleanApi.Template.Application.Features.Auth.Commands.Logout;

/// <summary>
/// Command payload for logging out an authenticated user and revoking active refresh session.
/// </summary>
public sealed record LogoutCommand() : IRequest<BaseResponse<LogoutCommandResponse>>;
