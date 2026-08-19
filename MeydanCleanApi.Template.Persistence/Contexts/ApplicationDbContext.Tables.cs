namespace MeydanCleanApi.Template.Persistence.Contexts;

/// <summary>
/// Table DbSet declarations for <see cref="ApplicationDbContext"/>, separated from context behaviors.
/// </summary>
public partial class ApplicationDbContext
{
    // Auths
    /// <summary>Lookup table for authentication providers (Google, Local, etc.).</summary>
    public DbSet<AuthProvider> AuthProviders { get; set; }

    /// <summary>Links user accounts to their authentication providers.</summary>
    public DbSet<UserAuthProvider> UserAuthProviders { get; set; }

    // Files
    /// <summary>Direct-to-cloud file upload metadata records.</summary>
    public DbSet<FileEntity> FileEntities { get; set; }

    /// <summary>Lookup table for the role a file plays for its owner (Gallery, Thumbnail, Datasheet).</summary>
    public DbSet<FilePurpose> FilePurposes { get; set; }

    /// <summary>Lookup table for file upload statuses (Pending, Uploaded, Failed).</summary>
    public DbSet<FileStatus> FileStatuses { get; set; }

    /// <summary>Lookup table for storage providers (Local, Supabase, etc.).</summary>
    public DbSet<FileStorage> FileStorages { get; set; }

    // Identity
    /// <summary>Application user accounts.</summary>
    public DbSet<AppUser> AppUsers { get; set; }

    /// <summary>Application user roles.</summary>
    public DbSet<AppRole> AppRoles { get; set; }

    // Cultures
    /// <summary>Supported application cultures for request localization.</summary>
    public DbSet<SupportedCulture> SupportedCultures { get; set; }

    //#if (IncludeSamples)
    // Samples (Reference Module)
    /// <summary>Sample products catalog.</summary>
    public DbSet<SampleProduct> SampleProducts { get; set; }

    /// <summary>Localized title and slug translations for sample products.</summary>
    public DbSet<SampleProductTranslation> SampleProductTranslations { get; set; }

    /// <summary>Sample product catalog categories.</summary>
    public DbSet<SampleProductCategory> SampleProductCategories { get; set; }

    /// <summary>Localized name and slug translations for sample product categories.</summary>
    public DbSet<SampleProductCategoryTranslation> ProductCategoryTranslations { get; set; }

    /// <summary>Specification attributes for sample products.</summary>
    public DbSet<SampleProductAttribute> SampleProductAttributes { get; set; }

    /// <summary>Localized name and value translations for sample product attributes.</summary>
    public DbSet<SampleProductAttributeTranslation> SampleProductAttributeTranslations { get; set; }

    /// <summary>Join entity linking sample products to uploaded files.</summary>
    public DbSet<SampleProductFile> SampleProductFiles { get; set; }
    //#endif
}
