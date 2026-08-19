using Microsoft.Extensions.Logging;
using MeydanCleanApi.Template.Domain.Constants;
using MeydanCleanApi.Template.Persistence.Contexts;
using MeydanCleanApi.Template.Persistence.Seeding.Base;

namespace MeydanCleanApi.Template.Persistence.Seeding;

/// <summary>
/// Database seeder service for seeding sample product catalog reference data.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SampleProductSeederService"/> class.
/// </remarks>
/// <param name="context">The database context instance.</param>
/// <param name="logger">Logger instance.</param>
public sealed class SampleProductSeederService(ApplicationDbContext context, ILogger<SampleProductSeederService> logger) : BaseSeederService<SampleProduct>(context, logger)
{

    /// <inheritdoc />
    protected override async Task<List<SampleProduct>> GetNewEntitiesAsync(CancellationToken ct = default)
    {
        if (await Context.SampleProducts.AnyAsync(ct))
        {
            return [];
        }

        var category = new SampleProductCategory
        {
            Id = Guid.NewGuid(),
            Name = "Electronics",
            IsActive = true,
            Translations =
            [
                new SampleProductCategoryTranslation { Id = Guid.NewGuid(), CultureCode = CultureConstants.TurkishCode, Title = "Elektronik", Description = "Elektronik ürünler", Slug = "elektronik" },
                new SampleProductCategoryTranslation { Id = Guid.NewGuid(), CultureCode = CultureConstants.EnglishUsCode, Title = "Electronics", Description = "Electronic products", Slug = "electronics" }
            ]
        };

        var product = new SampleProduct
        {
            Id = Guid.NewGuid(),
            SampleProductCategoryId = category.Id,
            Name = "Sample Smartphone",
            Price = 999.99m,
            IsActive = true,
            Translations =
            [
                new SampleProductTranslation { Id = Guid.NewGuid(), CultureCode = CultureConstants.TurkishCode, Title = "Örnek Akıllı Telefon", Description = "Yüksek performanslı örnek akıllı telefon", Slug = "ornek-akilli-telefon" },
                new SampleProductTranslation { Id = Guid.NewGuid(), CultureCode = CultureConstants.EnglishUsCode, Title = "Sample Smartphone", Description = "High performance sample smartphone", Slug = "sample-smartphone" }
            ]
        };

        await Context.SampleProductCategories.AddAsync(category, ct);
        return [product];
    }

    /// <inheritdoc />
    protected override string GetEntityInfo(SampleProduct entity)
    {
        return $"{nameof(SampleProduct)} [Id: {entity.Id}, Name: '{entity.Name}', Price: {entity.Price:C}]";
    }
}
