using MeydanCleanApi.Template.Application.Abstractions.Localization;
using MeydanCleanApi.Template.Application.Abstractions.Messaging;

namespace MeydanCleanApi.Template.Application.Features.Cultures.Queries.GetSupportedCultures;

/// <summary>
/// Query handler returning the active supported culture codes.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetSupportedCulturesQueryHandler"/> class.
/// </remarks>
public sealed class GetSupportedCulturesQueryHandler(
    ISupportedCultureProvider cultureProvider) : IRequestHandler<GetSupportedCulturesQuery, BaseResponse<GetSupportedCulturesQueryResponse>>
{
    private readonly ISupportedCultureProvider _cultureProvider = cultureProvider;

    /// <inheritdoc />
    public async Task<BaseResponse<GetSupportedCulturesQueryResponse>> Handle(
        GetSupportedCulturesQuery request, CancellationToken ct)
    {
        // The provider caches the list, so repeated calls do not hit the database every time.
        var cultures = await _cultureProvider.GetActiveCodesAsync(ct);

        return BaseResponse<GetSupportedCulturesQueryResponse>.Success(
            new GetSupportedCulturesQueryResponse(cultures));
    }
}
