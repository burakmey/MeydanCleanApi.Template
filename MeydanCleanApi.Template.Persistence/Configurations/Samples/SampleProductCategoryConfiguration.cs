using MeydanCleanApi.Template.Domain.Constants;
using MeydanCleanApi.Template.Persistence.Contexts;

namespace MeydanCleanApi.Template.Persistence.Configurations.Samples;

/// <summary>
/// EF Core configuration for <see cref="SampleProductCategory"/> entity.
/// </summary>
public sealed class SampleProductCategoryConfiguration : IEntityTypeConfiguration<SampleProductCategory>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SampleProductCategory> builder)
    {
        builder.ToTable(nameof(ApplicationDbContext.SampleProductCategories));

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(ValidationConstants.MaxCategoryNameLength);

        // 1:N Self-Referencing Relationship: Parent Category (1) -> Child Categories (N)
        // DeleteBehavior.Restrict: Prevents deletion of a parent category if child categories exist under it
        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // 1:N Relationship: SampleProductCategory (1) -> SampleProductCategoryTranslation (N)
        // DeleteBehavior.Cascade: Deleting a category automatically deletes all its translations
        builder.HasMany(x => x.Translations)
            .WithOne(t => t.SampleProductCategory)
            .HasForeignKey(t => t.SampleProductCategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
