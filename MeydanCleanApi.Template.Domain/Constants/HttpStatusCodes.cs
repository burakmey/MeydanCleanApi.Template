namespace MeydanCleanApi.Template.Domain.Constants;

/// <summary>
/// Defines integer HTTP status codes to eliminate magic numbers across handlers and middleware.
/// </summary>
public static class HttpStatusCodes
{
    /// <summary>HTTP 200 OK.</summary>
    public const int Ok = 200;

    /// <summary>HTTP 201 Created.</summary>
    public const int Created = 201;

    /// <summary>HTTP 400 Bad Request.</summary>
    public const int BadRequest = 400;

    /// <summary>HTTP 401 Unauthorized.</summary>
    public const int Unauthorized = 401;

    /// <summary>HTTP 403 Forbidden.</summary>
    public const int Forbidden = 403;

    /// <summary>HTTP 404 Not Found.</summary>
    public const int NotFound = 404;

    /// <summary>HTTP 409 Conflict.</summary>
    public const int Conflict = 409;

    /// <summary>HTTP 500 Internal Server Error.</summary>
    public const int InternalServerError = 500;
}
