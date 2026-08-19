namespace MeydanCleanApi.Template.Persistence.Configurations.Base;

/// <summary>
/// Shared EF Core mapping for every attachment table.
/// </summary>
/// <typeparam name="TAttachment">The concrete attachment entity, for example <c>SampleProductFile</c>.</typeparam>
/// <remarks>
/// Derive from this and you only have to configure what is specific to your aggregate: the owner
/// foreign key and any extra columns. Everything below is the same for all of them, so a new
/// attachment table stays about ten lines.
/// </remarks>
public abstract class BaseFileAttachmentConfiguration<TAttachment> : IEntityTypeConfiguration<TAttachment>
    where TAttachment : BaseFileAttachment
{
    /// <inheritdoc />
    public virtual void Configure(EntityTypeBuilder<TAttachment> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Id just reads and writes FileEntityId, so it must not become a column of its own.
        // Without this, EF Core maps both and the table ends up with a redundant Id column that
        // silently drifts away from the key it is supposed to mirror.
        builder.Ignore(x => x.Id);

        // The file id is the primary key, so an attachment row and its file share one identifier.
        builder.HasKey(x => x.FileEntityId);

        // 1:1 with the file. Restrict, because deleting file metadata while an owner still points at
        // it would leave a row referring to something that no longer exists.
        builder.HasOne(x => x.FileEntity)
            .WithOne()
            .HasForeignKey<TAttachment>(x => x.FileEntityId)
            .OnDelete(DeleteBehavior.Restrict);

        // Purpose is a real foreign key, so the database rejects a value that is not a known role.
        builder.HasOne(x => x.FilePurpose)
            .WithMany()
            .HasForeignKey(x => x.FilePurposeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Listing "the gallery images for this product, in order" is the common read, so index for it.
        builder.HasIndex(x => new { x.FilePurposeId, x.SortOrder });

        ConfigureAttachment(builder);
    }

    /// <summary>
    /// Override to map the owner foreign key and any fields specific to this attachment.
    /// </summary>
    /// <param name="builder">Builder for the concrete attachment entity.</param>
    protected abstract void ConfigureAttachment(EntityTypeBuilder<TAttachment> builder);
}
