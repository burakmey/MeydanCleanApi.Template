using MeydanCleanApi.Template.Persistence.Contexts;

namespace MeydanCleanApi.Template.Persistence.Configurations.Identity;

/// <summary>
/// EF Core configuration for <see cref="AppRole"/> identity entity.
/// </summary>
public sealed class AppRoleConfiguration : IEntityTypeConfiguration<AppRole>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AppRole> builder)
    {
        builder.ToTable(nameof(ApplicationDbContext.AppRoles));
    }
}
