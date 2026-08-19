using MeydanCleanApi.Template.Application.Common.Models;
using MeydanCleanApi.Template.Application.Features.Auth.Commands.ExternalLogin;
using MeydanCleanApi.Template.Application.Features.Auth.Commands.Login;
using MeydanCleanApi.Template.Application.Features.Auth.Commands.Logout;
using MeydanCleanApi.Template.Application.Features.Auth.Commands.RefreshToken;
using MeydanCleanApi.Template.WebApi.Controllers.Base;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.RateLimiting;

namespace MeydanCleanApi.Template.WebApi.Controllers.Auth;

/// <summary>
/// Controller handling user authentication, refresh tokens, and external OAuth logins via CQRS commands.
/// </summary>
/// <remarks>
/// The refresh token is issued as an HttpOnly cookie by the handlers, not returned in the response body.
/// Every endpoint here is rate limited to slow down password guessing.
/// </remarks>
[EnableRateLimiting(ServiceRegistration.AuthRateLimitPolicy)]
public sealed class AuthController : ApiControllerBase
{
    /// <summary>
    /// Authenticates a user with email and password credentials.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<LoginCommandResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login([FromBody] LoginCommand request, CancellationToken ct)
    {
        var result = await Mediator.Send(request, ct);
        return Ok(result);
    }

    /// <summary>
    /// Renews an access token using the refresh token stored in the HttpOnly cookie.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<RefreshTokenCommandResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Refresh(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] RefreshTokenCommand? request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(request ?? new RefreshTokenCommand(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Revokes the current session and clears the refresh token cookie.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(BaseResponse<LogoutCommandResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var result = await Mediator.Send(new LogoutCommand(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Authenticates a user via an external OAuth provider (e.g. Google).
    /// </summary>
    [HttpPost("external-login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<ExternalLoginCommandResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginCommand request, CancellationToken ct)
    {
        var result = await Mediator.Send(request, ct);
        return Ok(result);
    }
}
