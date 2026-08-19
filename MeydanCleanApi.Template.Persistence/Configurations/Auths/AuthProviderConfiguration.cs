using MeydanCleanApi.Template.Domain.Constants;
using MeydanCleanApi.Template.Persistence.Contexts;

namespace MeydanCleanApi.Template.Persistence.Configurations.Auths;

/// <summary>
/// EF Core configuration for <see cref="AuthProvider"/> lookup entity.
/// </summary>
public sealed class AuthProviderConfiguration : IEntityTypeConfiguration<AuthProvider>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AuthProvider> builder)
    {
        builder.ToTable(nameof(ApplicationDbContext.AuthProviders));

        builder.HasKey(x => x.Id);

        // ValueGeneratedNever prevents database auto-increment identity creation because IDs are fixed enum values
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(ValidationConstants.MaxShortNameLength);
    }
}
