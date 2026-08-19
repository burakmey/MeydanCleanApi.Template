using MeydanCleanApi.Template.Domain.Constants;
using MeydanCleanApi.Template.Persistence.Contexts;

namespace MeydanCleanApi.Template.Persistence.Configurations.Files;

/// <summary>
/// EF Core configuration for <see cref="FileStatus"/> lookup entity.
/// </summary>
public sealed class FileStatusConfiguration : IEntityTypeConfiguration<FileStatus>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<FileStatus> builder)
    {
        builder.ToTable(nameof(ApplicationDbContext.FileStatuses));

        builder.HasKey(x => x.Id);

        // ValueGeneratedNever prevents database auto-increment identity creation because status IDs are enum values
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(ValidationConstants.MaxShortNameLength);
    }
}
