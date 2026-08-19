using FluentValidation;
using MeydanCleanApi.Template.Application.Common.Constants;
using MeydanCleanApi.Template.Domain.Constants;

namespace MeydanCleanApi.Template.Application.Features.Files.Queries.GetFilesPaged;

/// <summary>
/// FluentValidation rules for <see cref="GetFilesPagedQuery"/>.
/// </summary>
public sealed class GetFilesPagedQueryValidator : AbstractValidator<GetFilesPagedQuery>
{
    /// <summary>
    /// Initializes validation rules for paged file queries.
    /// </summary>
    public GetFilesPagedQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(PaginationConstants.DefaultPageNumber)
            .WithMessage(ValidationCodes.PageNumberInvalid);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, PaginationConstants.MaxPageSize)
            .WithMessage(ValidationCodes.PageSizeInvalid);

        RuleFor(x => x.SearchTerm)
            .MaximumLength(ValidationConstants.MaxNameLength)
            .WithMessage(ValidationCodes.SearchTermTooLong)
            .When(x => x.SearchTerm is not null);
    }
}
