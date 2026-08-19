using MeydanCleanApi.Template.Domain.Constants;
using MeydanCleanApi.Template.Persistence.Contexts;

namespace MeydanCleanApi.Template.Persistence.Configurations.Samples;

/// <summary>
/// EF Core configuration for <see cref="SampleProductTranslation"/> entity.
/// </summary>
public sealed class SampleProductTranslationConfiguration : IEntityTypeConfiguration<SampleProductTranslation>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SampleProductTranslation> builder)
    {
        builder.ToTable(nameof(ApplicationDbContext.SampleProductTranslations));

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CultureCode)
            .IsRequired()
            .HasMaxLength(ValidationConstants.MaxCultureCodeLength);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(ValidationConstants.MaxNameLength);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(ValidationConstants.MaxDescriptionLength);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(ValidationConstants.MaxSlugLength);

        // Matches the soft-delete filter on the parent product. Without it EF warns that a required
        // relationship points at a filtered entity, and translations of a deactivated product would
        // still show up in queries.
        builder.HasQueryFilter(x => x.SampleProduct!.IsActive);


        builder.HasIndex(x => new { x.SampleProductId, x.CultureCode })
            .IsUnique();
    }
}
