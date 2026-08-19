using MeydanCleanApi.Template.Application.DTOs.Samples;
using MeydanCleanApi.Template.Domain.Entities.Samples;
using MeydanCleanApi.Template.Domain.Enums;

namespace MeydanCleanApi.Template.Application.Common.Extensions;

/// <summary>
/// High-performance static extension mapping methods for <see cref="SampleProduct"/> and <see cref="SampleProductFile"/>.
/// </summary>
public static class SampleProductMappingExtensions
{
    /// <summary>
    /// Projects a <see cref="SampleProduct"/> entity instance to a <see cref="SampleProductDto"/>.
    /// </summary>
    /// <param name="entity">Source entity.</param>
    /// <returns>Mapped <see cref="SampleProductDto"/> instance.</returns>
    public static SampleProductDto ToDto(this SampleProduct entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new SampleProductDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Price = entity.Price,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            // Ordered by purpose then position, so gallery images arrive in the sequence
            // somebody arranged them in rather than in whatever order the database returned.
            Files = entity.Files?
                .OrderBy(f => f.FilePurposeId)
                .ThenBy(f => f.SortOrder)
                .Select(f => new SampleProductFileDto
                {
                    FileId = f.FileEntityId,
                    Purpose = (FilePurposeType)f.FilePurposeId,
                    SortOrder = f.SortOrder
                }).ToList() ?? []
        };
    }
}
