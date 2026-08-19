using MeydanCleanApi.Template.Domain.Constants;
using MeydanCleanApi.Template.Persistence.Contexts;

namespace MeydanCleanApi.Template.Persistence.Configurations.Samples;

/// <summary>
/// EF Core configuration for <see cref="SampleProductCategoryTranslation"/> entity.
/// </summary>
public sealed class SampleProductCategoryTranslationConfiguration : IEntityTypeConfiguration<SampleProductCategoryTranslation>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SampleProductCategoryTranslation> builder)
    {
        builder.ToTable(nameof(ApplicationDbContext.ProductCategoryTranslations));

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CultureCode)
            .IsRequired()
            .HasMaxLength(ValidationConstants.MaxCultureCodeLength);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(ValidationConstants.MaxCategoryNameLength);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(ValidationConstants.MaxDescriptionLength);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(ValidationConstants.MaxNameLength);

        // Matches the soft-delete filter on the parent category, so translations of a deactivated
        // category are hidden too and EF does not warn about the required relationship.
        builder.HasQueryFilter(x => x.SampleProductCategory!.IsActive);


        builder.HasIndex(x => new { x.SampleProductCategoryId, x.CultureCode })
            .IsUnique();
    }
}
