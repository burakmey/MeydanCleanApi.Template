using MeydanCleanApi.Template.Domain.Constants;
using MeydanCleanApi.Template.Persistence.Contexts;

namespace MeydanCleanApi.Template.Persistence.Configurations.Samples;

/// <summary>
/// EF Core configuration for <see cref="SampleProductAttributeTranslation"/> entity.
/// </summary>
public sealed class SampleProductAttributeTranslationConfiguration : IEntityTypeConfiguration<SampleProductAttributeTranslation>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SampleProductAttributeTranslation> builder)
    {
        builder.ToTable(nameof(ApplicationDbContext.SampleProductAttributeTranslations));

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CultureCode)
            .IsRequired()
            .HasMaxLength(ValidationConstants.MaxCultureCodeLength);

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(ValidationConstants.MaxTitleLength);

        builder.Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(ValidationConstants.MaxAttributeValueLength);

        // Matches the soft-delete filter that reaches this entity through its parent attribute.
        builder.HasQueryFilter(x => x.SampleProductAttribute!.SampleProduct!.IsActive);


        builder.HasIndex(x => new { x.SampleProductAttributeId, x.CultureCode })
            .IsUnique();
    }
}
