using MeydanCleanApi.Template.Domain.Constants;
using MeydanCleanApi.Template.Persistence.Contexts;

namespace MeydanCleanApi.Template.Persistence.Configurations.Files;

/// <summary>
/// EF Core configuration for <see cref="FileStorage"/> lookup entity.
/// </summary>
public sealed class FileStorageConfiguration : IEntityTypeConfiguration<FileStorage>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<FileStorage> builder)
    {
        builder.ToTable(nameof(ApplicationDbContext.FileStorages));

        builder.HasKey(x => x.Id);

        // ValueGeneratedNever prevents database auto-increment identity creation because storage IDs are enum values
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(ValidationConstants.MaxShortNameLength);
    }
}
