using MeydanCleanApi.Template.Persistence.Contexts;

namespace MeydanCleanApi.Template.Persistence.Configurations.Samples;

/// <summary>
/// EF Core configuration for <see cref="SampleProductAttribute"/> entity.
/// </summary>
public sealed class SampleProductAttributeConfiguration : IEntityTypeConfiguration<SampleProductAttribute>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SampleProductAttribute> builder)
    {
        builder.ToTable(nameof(ApplicationDbContext.SampleProductAttributes));

        builder.HasKey(x => x.Id);

        // Matches the soft-delete filter on the parent product.
        builder.HasQueryFilter(x => x.SampleProduct!.IsActive);


        // 1:N Relationship: SampleProductAttribute (1) -> SampleProductAttributeTranslation (N)
        // DeleteBehavior.Cascade: Deleting a product attribute automatically deletes all its translations
        builder.HasMany(x => x.Translations)
            .WithOne(t => t.SampleProductAttribute)
            .HasForeignKey(t => t.SampleProductAttributeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
