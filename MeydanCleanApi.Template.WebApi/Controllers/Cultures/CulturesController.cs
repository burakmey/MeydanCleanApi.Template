using MeydanCleanApi.Template.Application.Common.Models;
using MeydanCleanApi.Template.Application.Features.Cultures.Queries.GetSupportedCultures;
using MeydanCleanApi.Template.WebApi.Controllers.Base;

namespace MeydanCleanApi.Template.WebApi.Controllers.Cultures;

/// <summary>
/// Controller for querying active supported culture codes.
/// </summary>
public sealed class CulturesController : ApiControllerBase
{
    /// <summary>
    /// Gets the list of supported culture codes (e.g. "tr-TR", "en-US").
    /// </summary>
    /// <remarks>
    /// Anonymous because clients need it before signing in, to pick a language for the login screen.
    /// </remarks>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<GetSupportedCulturesQueryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSupportedCultures(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetSupportedCulturesQuery(), ct);
        return Ok(result);
    }
}
