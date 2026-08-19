using MeydanCleanApi.Template.Persistence.Configurations.Base;
using MeydanCleanApi.Template.Persistence.Contexts;

namespace MeydanCleanApi.Template.Persistence.Configurations.Samples;

/// <summary>
/// EF Core configuration for the <see cref="SampleProductFile"/> attachment table.
/// </summary>
/// <remarks>
/// Everything shared with other attachment tables — the key, the file and purpose relationships, the
/// ordering index — comes from <see cref="BaseFileAttachmentConfiguration{TAttachment}"/>. Only the
/// product-specific parts are here.
/// </remarks>
public sealed class SampleProductFileConfiguration : BaseFileAttachmentConfiguration<SampleProductFile>
{
    /// <inheritdoc />
    protected override void ConfigureAttachment(EntityTypeBuilder<SampleProductFile> builder)
    {
        builder.ToTable(nameof(ApplicationDbContext.SampleProductFiles));

        // 1:N Relationship: SampleProduct (1) -> SampleProductFile (N)
        // DeleteBehavior.Restrict: a product cannot be deleted while files are still attached.
        // Detach them first so the stored objects get cleaned up rather than orphaned.
        builder.HasOne(x => x.SampleProduct)
            .WithMany(p => p.Files)
            .HasForeignKey(x => x.SampleProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Reading "all files for this product, grouped by role, in order" is the query this table exists for.
        builder.HasIndex(x => new { x.SampleProductId, x.FilePurposeId, x.SortOrder });

        // Matches the soft-delete filter on the parent product.
        builder.HasQueryFilter(x => x.SampleProduct!.IsActive);
    }
}
