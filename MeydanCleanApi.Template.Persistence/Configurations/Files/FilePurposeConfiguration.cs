using MeydanCleanApi.Template.Domain.Constants;
using MeydanCleanApi.Template.Persistence.Contexts;

namespace MeydanCleanApi.Template.Persistence.Configurations.Files;

/// <summary>
/// EF Core configuration for the <see cref="FilePurpose"/> lookup entity.
/// </summary>
public sealed class FilePurposeConfiguration : IEntityTypeConfiguration<FilePurpose>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<FilePurpose> builder)
    {
        builder.ToTable(nameof(ApplicationDbContext.FilePurposes));

        builder.HasKey(x => x.Id);

        // ValueGeneratedNever because the ids come from the FilePurposeType enum, not the database.
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(ValidationConstants.MaxShortNameLength);
    }
}
