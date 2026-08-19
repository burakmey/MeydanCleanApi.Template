using MeydanCleanApi.Template.Application.Common.Models.Pagination;
using MeydanCleanApi.Template.Application.DTOs.Samples;

namespace MeydanCleanApi.Template.Application.Features.Samples.Queries.GetSampleProductsPaged;

/// <summary>
/// Response payload for <see cref="GetSampleProductsPagedQuery"/> carrying paged product records.
/// </summary>
/// <param name="Products">Paged collection response containing <see cref="SampleProductDto"/> items.</param>
public record GetSampleProductsPagedQueryResponse(
    PagedResponse<SampleProductDto> Products);
