namespace MeydanCleanApi.Template.WebApi.Controllers.Base;

/// <summary>
/// Abstract base controller for public/application API endpoints providing custom <see cref="IMediator"/> access.
/// </summary>
/// <remarks>
/// Actions return <c>BaseResponse&lt;T&gt;</c> directly. There is deliberately no helper that wraps results in
/// an anonymous object: two different response shapes in one API means clients cannot write a single
/// deserializer, and anonymous objects give Swagger nothing to document.
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private IMediator? _mediator;

    /// <summary>
    /// Gets the lazy-loaded custom <see cref="IMediator"/> instance from the HTTP request scope.
    /// </summary>
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();
}
