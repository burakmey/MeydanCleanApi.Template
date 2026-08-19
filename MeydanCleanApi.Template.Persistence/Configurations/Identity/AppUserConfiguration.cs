using MeydanCleanApi.Template.Domain.Constants;
using MeydanCleanApi.Template.Persistence.Contexts;

namespace MeydanCleanApi.Template.Persistence.Configurations.Identity;

/// <summary>
/// EF Core configuration for <see cref="AppUser"/> identity entity.
/// </summary>
public sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable(nameof(ApplicationDbContext.AppUsers));

        builder.Property(x => x.RefreshTokenHash)
            .HasMaxLength(ValidationConstants.MaxTokenLength);

        builder.Property(x => x.CurrentJti)
            .HasMaxLength(ValidationConstants.MaxEmailLength);

        // Identity only checks email uniqueness in UserManager, which two requests arriving at the
        // same time can both pass. A unique index makes the database the final authority.
        builder.HasIndex(x => x.NormalizedEmail)
            .IsUnique();

        // Refresh tokens are looked up by hash on every token refresh.
        builder.HasIndex(x => x.RefreshTokenHash);

        // 1:N Relationship: AuthProvider (1) -> AppUser (N)
        // Ties ActiveAuthProviderId to the lookup table so it cannot hold a value that has no matching row.
        builder.HasOne<AuthProvider>()
            .WithMany()
            .HasForeignKey(x => x.ActiveAuthProviderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
