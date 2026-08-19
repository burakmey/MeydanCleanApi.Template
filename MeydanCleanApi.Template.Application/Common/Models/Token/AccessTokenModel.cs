namespace MeydanCleanApi.Template.Application.Common.Models.Token;

/// <summary>
/// The part of a <see cref="TokenModel"/> that is safe to send back in an API response body.
/// </summary>
/// <remarks>
/// The refresh token is deliberately not here. It travels only in the HttpOnly cookie set by
/// <c>AuthController</c>, so page scripts cannot read it. Putting it in the response body as well
/// would undo that protection, because any cross-site scripting flaw could then steal it.
/// </remarks>
/// <param name="AccessToken">Signed JWT to send in the <c>Authorization: Bearer</c> header.</param>
/// <param name="AccessTokenExpiration">Moment in UTC when the access token stops being accepted.</param>
public sealed record AccessTokenModel(string AccessToken, DateTime AccessTokenExpiration)
{
    /// <summary>
    /// Copies the client-safe fields out of a full <see cref="TokenModel"/>.
    /// </summary>
    /// <param name="token">The token pair produced when signing in.</param>
    /// <returns>A response-safe view of the token pair.</returns>
    public static AccessTokenModel From(TokenModel token)
    {
        ArgumentNullException.ThrowIfNull(token);
        return new AccessTokenModel(token.AccessToken, token.AccessTokenExpiration);
    }
}
