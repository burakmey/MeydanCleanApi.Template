using MeydanCleanApi.Template.Domain.Enums;

namespace MeydanCleanApi.Template.Application.Abstractions.Storage;

/// <summary>
/// Application storage contract for resolving registered <see cref="IFileStorageService"/> implementations by <see cref="FileStorageType"/>.
/// </summary>
public interface IFileStorageHandlerResolver
{
    /// <summary>
    /// Resolves the concrete storage provider service for the requested storage type.
    /// </summary>
    /// <param name="storageType">Target file storage type (e.g. Local, Supabase).</param>
    /// <returns>The registered <see cref="IFileStorageService"/> instance.</returns>
    IFileStorageService Resolve(FileStorageType storageType);
}
