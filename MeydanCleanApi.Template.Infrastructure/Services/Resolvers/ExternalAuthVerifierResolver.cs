using MeydanCleanApi.Template.Application.Abstractions.Auth;
using MeydanCleanApi.Template.Domain.Exceptions;

namespace MeydanCleanApi.Template.Infrastructure.Services.Resolvers;

/// <summary>
/// Infrastructure implementation of <see cref="IExternalAuthVerifierResolver"/>.
/// </summary>
/// <remarks>
/// Built the same way as <see cref="FileStorageHandlerResolver"/>: every registered verifier is
/// indexed once by the provider it handles, so lookups are a dictionary hit rather than a scan.
/// </remarks>
public sealed class ExternalAuthVerifierResolver : IExternalAuthVerifierResolver
{
    private readonly IReadOnlyDictionary<AuthProviderType, IExternalAuthVerifier> _verifiers;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalAuthVerifierResolver"/> class.
    /// </summary>
    /// <param name="verifiers">All registered external authentication verifiers.</param>
    public ExternalAuthVerifierResolver(IEnumerable<IExternalAuthVerifier> verifiers)
    {
        ArgumentNullException.ThrowIfNull(verifiers);
        _verifiers = verifiers.ToDictionary(verifier => verifier.AuthProvider);
    }

    /// <inheritdoc />
    public IExternalAuthVerifier Resolve(AuthProviderType authProvider)
    {
        // Local means email and password. It has no external token to verify.
        if (authProvider == AuthProviderType.Local)
        {
            throw ConflictException.WithCode(ErrorCodes.ExternalAuthProviderNotSupported);
        }

        if (_verifiers.TryGetValue(authProvider, out var verifier))
        {
            return verifier;
        }

        throw ConflictException.WithCode(ErrorCodes.ExternalAuthProviderNotSupported);
    }
}
