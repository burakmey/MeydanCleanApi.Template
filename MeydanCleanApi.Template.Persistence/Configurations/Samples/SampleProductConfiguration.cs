using MeydanCleanApi.Template.Domain.Constants;
using MeydanCleanApi.Template.Persistence.Contexts;

namespace MeydanCleanApi.Template.Persistence.Configurations.Samples;

/// <summary>
/// EF Core configuration for <see cref="SampleProduct"/> sample catalog entity.
/// </summary>
public sealed class SampleProductConfiguration : IEntityTypeConfiguration<SampleProduct>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SampleProduct> builder)
    {
        builder.ToTable(nameof(ApplicationDbContext.SampleProducts));

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(ValidationConstants.MaxNameLength);

        builder.Property(x => x.Price)
            .HasPrecision(18, 2);

        // 1:N Relationship: SampleProductCategory (1) -> SampleProduct (N)
        // Configured explicitly because the EF Core default for a required foreign key is Cascade,
        // which would delete every product in a category the moment that category is removed.
        builder.HasOne(x => x.SampleProductCategory)
            .WithMany(c => c.Products)
            .HasForeignKey(x => x.SampleProductCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // 1:N Relationship: SampleProduct (1) -> SampleProductTranslation (N)
        // DeleteBehavior.Cascade: Deleting a product automatically cascades and deletes all its localized translations
        builder.HasMany(x => x.Translations)
            .WithOne(t => t.SampleProduct)
            .HasForeignKey(t => t.SampleProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // 1:N Relationship: SampleProduct (1) -> SampleProductFile (N)
        // DeleteBehavior.Restrict: Prevents accidental deletion of a product if attached files exist.
        // Handler must explicitly delete physical cloud files and file records before removing the product.
        builder.HasMany(x => x.Files)
            .WithOne(pf => pf.SampleProduct)
            .HasForeignKey(pf => pf.SampleProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // 1:N Relationship: SampleProduct (1) -> SampleProductAttribute (N)
        // DeleteBehavior.Cascade: Deleting a product cascades and removes all product specification attributes
        builder.HasMany(x => x.Attributes)
            .WithOne(pa => pa.SampleProduct)
            .HasForeignKey(pa => pa.SampleProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
