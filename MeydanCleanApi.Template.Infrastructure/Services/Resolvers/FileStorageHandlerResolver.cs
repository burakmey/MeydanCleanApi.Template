using MeydanCleanApi.Template.Application.Abstractions.Storage;
using MeydanCleanApi.Template.Domain.Exceptions;

namespace MeydanCleanApi.Template.Infrastructure.Services.Resolvers;

/// <summary>
/// Infrastructure service for resolving registered <see cref="IFileStorageService"/> implementations by <see cref="FileStorageType"/>.
/// </summary>
public sealed class FileStorageHandlerResolver : IFileStorageHandlerResolver
{
    private readonly IReadOnlyDictionary<FileStorageType, IFileStorageService> _fileStorageServices;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileStorageHandlerResolver"/> class.
    /// </summary>
    /// <param name="fileStorageServices">All registered file storage service implementations.</param>
    public FileStorageHandlerResolver(IEnumerable<IFileStorageService> fileStorageServices)
    {
        ArgumentNullException.ThrowIfNull(fileStorageServices);
        _fileStorageServices = fileStorageServices.ToDictionary(h => h.FileStorageType);
    }

    /// <inheritdoc />
    public IFileStorageService Resolve(FileStorageType storageType)
    {
        if (_fileStorageServices.TryGetValue(storageType, out var handler))
        {
            return handler;
        }

        throw ConfigurationMissingException.ForSection($"Storage:{storageType}");
    }
}
