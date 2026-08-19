using FluentValidation;
using MeydanCleanApi.Template.Domain.Constants;

namespace MeydanCleanApi.Template.Application.Features.Samples.Commands.CreateSampleProduct;

/// <summary>
/// FluentValidation rules for <see cref="CreateSampleProductCommand"/>.
/// Uses domain constants and generic locale-independent validation code keys.
/// </summary>
public sealed class CreateSampleProductCommandValidator : AbstractValidator<CreateSampleProductCommand>
{
    /// <summary>
    /// Initializes validation rules for product creation.
    /// </summary>
    public CreateSampleProductCommandValidator()
    {
        RuleFor(x => x.SampleProductCategoryId)
            .NotEmpty()
            .WithMessage(ValidationCodes.CategoryRequired);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(ValidationCodes.NameRequired)
            .MaximumLength(ValidationConstants.MaxNameLength)
            .WithMessage(ValidationCodes.NameMaxLength);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(ValidationConstants.MinPrice)
            .WithMessage(ValidationCodes.PriceInvalid);

        When(x => x.MainFile != null, () =>
        {
            RuleFor(x => x.MainFile!.OriginalFileName)
                .NotEmpty()
                .WithMessage(ValidationCodes.FileNameRequired);

            RuleFor(x => x.MainFile!.SizeInBytes)
                .GreaterThan(0)
                .WithMessage(ValidationCodes.FileSizeInvalid);
        });
    }
}
