using MeydanCleanApi.Template.Application.Abstractions.Messaging;

namespace MeydanCleanApi.Template.Application.Features.Auth.Commands.Login;

/// <summary>
/// Command payload for authenticating a user with email and password credentials.
/// </summary>
/// <param name="Email">User email address.</param>
/// <param name="Password">User plain text password.</param>
public sealed record LoginCommand(string Email, string Password) : IRequest<BaseResponse<LoginCommandResponse>>;
