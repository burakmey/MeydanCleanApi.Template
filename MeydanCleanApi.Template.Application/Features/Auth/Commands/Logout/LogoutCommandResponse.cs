namespace MeydanCleanApi.Template.Application.Features.Auth.Commands.Logout;

/// <summary>
/// Response payload returned after successful user logout operation.
/// </summary>
/// <param name="LoggedOut">Indicates if logout succeeded.</param>
public sealed record LogoutCommandResponse(bool LoggedOut = true);
