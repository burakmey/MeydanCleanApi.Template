using MeydanCleanApi.Template.Domain.Constants;
using MeydanCleanApi.Template.Persistence.Contexts;

namespace MeydanCleanApi.Template.Persistence.Configurations.Culture;

/// <summary>
/// EF Core configuration for <see cref="SupportedCulture"/> entity.
/// </summary>
public sealed class SupportedCultureConfiguration : IEntityTypeConfiguration<SupportedCulture>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SupportedCulture> builder)
    {
        builder.ToTable(nameof(ApplicationDbContext.SupportedCultures));

        builder.HasKey(x => x.Id);

        // ValueGeneratedNever prevents database auto-increment identity creation because culture IDs are static integer values
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.CultureCode)
            .IsRequired()
            .HasMaxLength(ValidationConstants.MaxCultureCodeLength);

        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(ValidationConstants.MaxTitleLength);

        builder.HasIndex(x => x.CultureCode)
            .IsUnique();
    }
}
