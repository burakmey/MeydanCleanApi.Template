using MeydanCleanApi.Template.Domain.Enums;

namespace MeydanCleanApi.Template.Application.DTOs.Samples;

/// <summary>
/// DTO representation of an attached <see cref="Domain.Entities.Samples.SampleProductFile"/>.
/// </summary>
public record SampleProductFileDto
{
    /// <summary>
    /// Gets or sets the file entity identifier.
    /// </summary>
    public required Guid FileId { get; init; }

    /// <summary>
    /// Gets or sets the role this file plays for the product, such as a gallery image or a datasheet.
    /// </summary>
    public required FilePurposeType Purpose { get; init; }

    /// <summary>
    /// Gets or sets the display order within the same purpose.
    /// </summary>
    public required int SortOrder { get; init; }
}
