using FluentValidation;
using MeydanCleanApi.Template.Application.Common.Constants;
using MeydanCleanApi.Template.Domain.Constants;

namespace MeydanCleanApi.Template.Application.Features.Samples.Queries.GetSampleProductsPaged;

/// <summary>
/// FluentValidation rules for <see cref="GetSampleProductsPagedQuery"/>.
/// Uses pagination constants and locale-independent validation code keys.
/// </summary>
public sealed class GetSampleProductsPagedQueryValidator : AbstractValidator<GetSampleProductsPagedQuery>
{
    /// <summary>
    /// Initializes validation rules for paged product queries.
    /// </summary>
    public GetSampleProductsPagedQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(PaginationConstants.DefaultPageNumber)
            .WithMessage(ValidationCodes.PageNumberInvalid);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, PaginationConstants.MaxPageSize)
            .WithMessage(ValidationCodes.PageSizeInvalid);
    }
}
