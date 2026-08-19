using FluentValidation;
using MeydanCleanApi.Template.Domain.Constants;

namespace MeydanCleanApi.Template.Application.Features.Samples.Commands.SoftDeleteSampleProduct;

/// <summary>
/// FluentValidation rules for <see cref="SoftDeleteSampleProductCommand"/>.
/// </summary>
public sealed class SoftDeleteSampleProductCommandValidator : AbstractValidator<SoftDeleteSampleProductCommand>
{
    /// <summary>
    /// Initializes validation rules for deactivating a product.
    /// </summary>
    public SoftDeleteSampleProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(ValidationCodes.IdRequired);
    }
}
