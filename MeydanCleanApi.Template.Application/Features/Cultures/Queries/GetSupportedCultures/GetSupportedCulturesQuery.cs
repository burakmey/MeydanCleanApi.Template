using MeydanCleanApi.Template.Application.Abstractions.Messaging;

namespace MeydanCleanApi.Template.Application.Features.Cultures.Queries.GetSupportedCultures;

/// <summary>
/// Query returning the culture codes this API currently serves content in.
/// </summary>
public sealed record GetSupportedCulturesQuery : IRequest<BaseResponse<GetSupportedCulturesQueryResponse>>;
