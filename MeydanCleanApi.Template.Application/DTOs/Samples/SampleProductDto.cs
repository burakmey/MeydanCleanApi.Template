namespace MeydanCleanApi.Template.Application.DTOs.Samples;

/// <summary>
/// DTO representation of a <see cref="Domain.Entities.Samples.SampleProduct"/> entity.
/// </summary>
public record SampleProductDto
{
    /// <summary>
    /// Gets or sets the product identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the product name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the unit price.
    /// </summary>
    public required decimal Price { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the product is active.
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Gets or sets the creation timestamp in UTC.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets or sets the collection of attached product files.
    /// </summary>
    public IReadOnlyList<SampleProductFileDto> Files { get; init; } = [];
}
