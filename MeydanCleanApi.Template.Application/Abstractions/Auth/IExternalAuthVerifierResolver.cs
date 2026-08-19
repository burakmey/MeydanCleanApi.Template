using MeydanCleanApi.Template.Domain.Enums;

namespace MeydanCleanApi.Template.Application.Abstractions.Auth;

/// <summary>
/// Resolves the registered <see cref="IExternalAuthVerifier"/> for a sign-in provider.
/// </summary>
/// <remarks>
/// Mirrors <see cref="Storage.IFileStorageHandlerResolver"/>. Adding Apple or Microsoft sign-in then
/// means writing one verifier and registering it — no existing code changes, and callers never scan a
/// list or decide what "unsupported provider" should mean.
/// </remarks>
public interface IExternalAuthVerifierResolver
{
    /// <summary>
    /// Returns the verifier registered for <paramref name="authProvider"/>.
    /// </summary>
    /// <param name="authProvider">Target provider, for example <see cref="AuthProviderType.Google"/>.</param>
    /// <returns>The matching verifier.</returns>
    /// <exception cref="Domain.Exceptions.ConflictException">
    /// Thrown when no verifier is registered for the provider, or when <see cref="AuthProviderType.Local"/>
    /// is requested — that is email and password, not an external provider.
    /// </exception>
    IExternalAuthVerifier Resolve(AuthProviderType authProvider);
}
