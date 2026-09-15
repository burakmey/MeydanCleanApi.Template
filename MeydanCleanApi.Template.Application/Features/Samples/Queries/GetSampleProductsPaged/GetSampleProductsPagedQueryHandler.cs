using System.Linq.Expressions;
using MeydanCleanApi.Template.Application.Abstractions.Localization;
using MeydanCleanApi.Template.Application.Abstractions.Messaging;
using MeydanCleanApi.Template.Application.Abstractions.Repositories;
using MeydanCleanApi.Template.Application.Common.Extensions;
using MeydanCleanApi.Template.Application.Localization;
using MeydanCleanApi.Template.Domain.Constants;
using MeydanCleanApi.Template.Domain.Entities.Samples;

namespace MeydanCleanApi.Template.Application.Features.Samples.Queries.GetSampleProductsPaged;

/// <summary>
/// Handler executing paged product search queries.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetSampleProductsPagedQueryHandler"/> class.
/// </remarks>
public sealed class GetSampleProductsPagedQueryHandler(
    IReadRepository<SampleProduct, Guid> productReadRepository,
    ILocalizationService<ApiMessages> localizer) : IRequestHandler<GetSampleProductsPagedQuery, BaseResponse<GetSampleProductsPagedQueryResponse>>
{
    private readonly IReadRepository<SampleProduct, Guid> _productReadRepository = productReadRepository;
    private readonly ILocalizationService<ApiMessages> _localizer = localizer;

    /// <inheritdoc />
    public async Task<BaseResponse<GetSampleProductsPagedQueryResponse>> Handle(GetSampleProductsPagedQuery request, CancellationToken ct)
    {
        // Lowered on both sides, because LIKE in PostgreSQL is case sensitive: matching the term as
        // typed would mean a search for "iphone" never finds a product stored as "iPhone". Lowering
        // the column also rules out the plain index on it, so a table large enough to feel that wants
        // an index on lower(name) to match.
        Expression<Func<SampleProduct, bool>>? filter = null;

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.Trim().ToLowerInvariant();
            filter = product => product.Name.ToLower().Contains(searchTerm);
        }

        // Files are part of the response DTO, so they have to be loaded with the products.
        // Without the include, EF returns an empty collection and the API silently reports that
        // every product has no attachments.
        //
        // Deactivated products never appear here: the global soft-delete query filter removes them.
        var pagedEntities = await _productReadRepository.GetPagedAsync(
            request.Page,
            predicate: filter,
            includes: [product => product.Files],
            ct: ct);

        var pagedDtos = pagedEntities.Map(product => product.ToDto());
        var queryResponse = new GetSampleProductsPagedQueryResponse(pagedDtos);

        // Compile-time safe entity name resolution using C# nameof keyword (zero hardcoded strings)
        var entityName = _localizer.Get(nameof(SampleProduct));
        var localizedMessage = _localizer.Get(ResponseCodes.Fetched, entityName);

        return BaseResponse<GetSampleProductsPagedQueryResponse>.Success(queryResponse, localizedMessage);
    }
}
