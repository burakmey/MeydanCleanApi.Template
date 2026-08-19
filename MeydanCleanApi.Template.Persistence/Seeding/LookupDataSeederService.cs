using Microsoft.Extensions.Logging;
using MeydanCleanApi.Template.Domain.Constants;
using MeydanCleanApi.Template.Domain.Enums;
using MeydanCleanApi.Template.Persistence.Contexts;

namespace MeydanCleanApi.Template.Persistence.Seeding;

/// <summary>
/// Unified database seeder for initializing all domain lookup tables (FileStatus, FileStorage, AuthProvider, SupportedCulture).
/// </summary>
public static class LookupDataSeederService
{
    /// <summary>
    /// Seeds initial lookup rows for storage statuses, storage providers, auth providers, and supported cultures with structured logging.
    /// </summary>
    public static async Task SeedAsync(ApplicationDbContext context, ILogger? logger = null, CancellationToken ct = default)
    {
        if (!await context.FileStatuses.AnyAsync(ct))
        {
            var fileStatuses = new FileStatus[]
            {
                new() { Id = (int)FileStatusType.Pending, Name = nameof(FileStatusType.Pending) },
                new() { Id = (int)FileStatusType.Uploaded, Name = nameof(FileStatusType.Uploaded) },
                new() { Id = (int)FileStatusType.Failed, Name = nameof(FileStatusType.Failed) }
            };

            foreach (var status in fileStatuses)
            {
                logger?.LogInformation("[Seeder] Seeding {EntityName}: Id={Id}, Name='{Name}'", nameof(FileStatus), status.Id, status.Name);
            }

            context.FileStatuses.AddRange(fileStatuses);
        }

        if (!await context.FilePurposes.AnyAsync(ct))
        {
            // One row per FilePurposeType member. Add a member there and a row here to introduce a
            // new kind of attachment — no new table is needed.
            var filePurposes = Enum.GetValues<FilePurposeType>()
                .Select(purpose => new FilePurpose { Id = (int)purpose, Name = purpose.ToString() })
                .ToArray();

            foreach (var purpose in filePurposes)
            {
                logger?.LogInformation("[Seeder] Seeding {EntityName}: Id={Id}, Name='{Name}'", nameof(FilePurpose), purpose.Id, purpose.Name);
            }

            context.FilePurposes.AddRange(filePurposes);
        }

        if (!await context.FileStorages.AnyAsync(ct))
        {
            var fileStorages = new FileStorage[]
            {
                new() { Id = (int)FileStorageType.Local, Name = nameof(FileStorageType.Local) },
                new() { Id = (int)FileStorageType.Supabase, Name = nameof(FileStorageType.Supabase) },
                new() { Id = (int)FileStorageType.AWS, Name = nameof(FileStorageType.AWS) },
                new() { Id = (int)FileStorageType.Google, Name = nameof(FileStorageType.Google) },
                new() { Id = (int)FileStorageType.Azure, Name = nameof(FileStorageType.Azure) }
            };

            foreach (var storage in fileStorages)
            {
                logger?.LogInformation("[Seeder] Seeding {EntityName}: Id={Id}, Name='{Name}'", nameof(FileStorage), storage.Id, storage.Name);
            }

            context.FileStorages.AddRange(fileStorages);
        }

        if (!await context.AuthProviders.AnyAsync(ct))
        {
            var authProviders = new AuthProvider[]
            {
                new() { Id = (int)AuthProviderType.Local, Name = nameof(AuthProviderType.Local) },
                new() { Id = (int)AuthProviderType.Google, Name = nameof(AuthProviderType.Google) },
                new() { Id = (int)AuthProviderType.Apple, Name = nameof(AuthProviderType.Apple) }
            };

            foreach (var provider in authProviders)
            {
                logger?.LogInformation("[Seeder] Seeding {EntityName}: Id={Id}, Name='{Name}'", nameof(AuthProvider), provider.Id, provider.Name);
            }

            context.AuthProviders.AddRange(authProviders);
        }

        if (!await context.SupportedCultures.AnyAsync(ct))
        {
            var cultures = new SupportedCulture[]
            {
                new() { Id = (int)SupportedCultureType.TrTR, CultureCode = CultureConstants.TurkishCode, DisplayName = CultureConstants.TurkishDisplayName, IsActive = true },
                new() { Id = (int)SupportedCultureType.EnUS, CultureCode = CultureConstants.EnglishUsCode, DisplayName = CultureConstants.EnglishUsDisplayName, IsActive = true }
            };

            foreach (var culture in cultures)
            {
                logger?.LogInformation("[Seeder] Seeding {EntityName}: Id={Id}, CultureCode='{CultureCode}', DisplayName='{DisplayName}'", nameof(SupportedCulture), culture.Id, culture.CultureCode, culture.DisplayName);
            }

            context.SupportedCultures.AddRange(cultures);
        }

        await context.SaveChangesAsync(ct);
    }
}
