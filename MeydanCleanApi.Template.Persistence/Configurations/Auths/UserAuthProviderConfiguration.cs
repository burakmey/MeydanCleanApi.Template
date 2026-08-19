using MeydanCleanApi.Template.Domain.Constants;
using MeydanCleanApi.Template.Persistence.Contexts;

namespace MeydanCleanApi.Template.Persistence.Configurations.Auths;

/// <summary>
/// EF Core configuration for <see cref="UserAuthProvider"/> entity linking users to external OAuth providers.
/// </summary>
public sealed class UserAuthProviderConfiguration : IEntityTypeConfiguration<UserAuthProvider>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UserAuthProvider> builder)
    {
        builder.ToTable(nameof(ApplicationDbContext.UserAuthProviders));

        // Id is a convenience alias over the two key columns, not a column of its own.
        // Without this, EF Core maps it as a real column of type "record", which PostgreSQL
        // rejects outright: 42P16 column "Id" has pseudo-type record.
        builder.Ignore(x => x.Id);

        // Composite Primary Key: UserId + AuthProviderId
        builder.HasKey(x => new { x.UserId, x.AuthProviderId });

        // ProviderKey stores the external OAuth subject identifier (OAuth 'sub' claim or local UserId string)
        builder.Property(x => x.ProviderKey)
            .IsRequired()
            .HasMaxLength(ValidationConstants.MaxProviderKeyLength);

        // 1:N Relationship: AppUser (1) -> UserAuthProvider (N)
        // DeleteBehavior.Cascade: Deleting a user removes all their external authentication provider links
        builder.HasOne(x => x.User)
            .WithMany(u => u.UserAuthProviders)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // 1:N Relationship: AuthProvider (1) -> UserAuthProvider (N)
        // DeleteBehavior.Restrict: Prevents deletion of an AuthProvider row if users are linked to it
        builder.HasOne(x => x.AuthProvider)
            .WithMany(p => p.UserAuthProviders)
            .HasForeignKey(x => x.AuthProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.AuthProviderId, x.ProviderKey })
            .IsUnique();
    }
}
