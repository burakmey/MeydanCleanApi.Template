namespace MeydanCleanApi.Template.Application.Common.Models;

/// <summary>
/// Universal response envelope returned by all CQRS command and query handlers to enforce a consistent API contract shape.
/// </summary>
/// <typeparam name="T">The payload data type carried by <see cref="Data"/>.</typeparam>
public class BaseResponse<T>
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSucceed { get; init; } = true;

    /// <summary>
    /// Gets or sets the response payload. Null for void operations or deleted entities.
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Gets or sets an optional localized user-facing message.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Creates a successful response envelope.
    /// </summary>
    /// <param name="data">The payload instance.</param>
    /// <param name="message">Optional localized message.</param>
    /// <returns>Populated <see cref="BaseResponse{T}"/> instance with <see cref="IsSucceed"/> = <c>true</c>.</returns>
    public static BaseResponse<T> Success(T? data = default, string? message = null)
        => new() { IsSucceed = true, Data = data, Message = message };

    /// <summary>
    /// Creates a failed response envelope.
    /// </summary>
    /// <param name="message">Localized description of the failure.</param>
    /// <returns>Populated <see cref="BaseResponse{T}"/> instance with <see cref="IsSucceed"/> = <c>false</c>.</returns>
    public static BaseResponse<T> Failure(string? message = null)
        => new() { IsSucceed = false, Message = message };
}
