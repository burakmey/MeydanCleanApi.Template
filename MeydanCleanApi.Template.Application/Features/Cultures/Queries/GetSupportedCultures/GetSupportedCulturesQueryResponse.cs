namespace MeydanCleanApi.Template.Application.Features.Cultures.Queries.GetSupportedCultures;

/// <summary>
/// Response payload listing the active culture codes.
/// </summary>
/// <param name="Cultures">Active culture codes, for example "tr-TR" and "en-US".</param>
public sealed record GetSupportedCulturesQueryResponse(IReadOnlyList<string> Cultures);
