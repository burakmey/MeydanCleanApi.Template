using MeydanCleanApi.Template.Domain.Constants;
using MeydanCleanApi.Template.Persistence.Contexts;

namespace MeydanCleanApi.Template.Persistence.Configurations.Files;

/// <summary>
/// EF Core configuration for <see cref="FileEntity"/> metadata entity.
/// </summary>
public sealed class FileEntityConfiguration : IEntityTypeConfiguration<FileEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<FileEntity> builder)
    {
        builder.ToTable(nameof(ApplicationDbContext.FileEntities));

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OriginalFileName)
            .IsRequired()
            .HasMaxLength(ValidationConstants.MaxFileNameLength);

        builder.Property(x => x.Path)
            .IsRequired()
            .HasMaxLength(ValidationConstants.MaxPathLength);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(ValidationConstants.MaxTitleLength);

        // Two records must never point at the same stored object: deleting one would remove the
        // file out from under the other. The database enforces this, not just the application code.
        builder.HasIndex(x => x.Path)
            .IsUnique();

        // PostgreSQL keeps a hidden "xmin" system column on every row, and its value changes on each
        // update. Mapping it as a row version costs no extra schema and makes EF Core notice when two
        // requests read the same file record and both try to change its status: the second save then
        // fails instead of silently overwriting the first.
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();

        // 1:N Relationship: FileStatus (1) -> FileEntity (N)
        // DeleteBehavior.Restrict: Prevents deletion of a FileStatus lookup row if file records depend on it
        builder.HasOne(x => x.FileStatus)
            .WithMany()
            .HasForeignKey(x => x.FileStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        // 1:N Relationship: FileStorage (1) -> FileEntity (N)
        // DeleteBehavior.Restrict: Prevents deletion of a FileStorage lookup row if file records depend on it
        builder.HasOne(x => x.FileStorage)
            .WithMany()
            .HasForeignKey(x => x.FileStorageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
