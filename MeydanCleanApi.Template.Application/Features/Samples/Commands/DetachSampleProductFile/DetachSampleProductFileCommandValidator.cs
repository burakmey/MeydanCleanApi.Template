using FluentValidation;
using MeydanCleanApi.Template.Domain.Constants;

namespace MeydanCleanApi.Template.Application.Features.Samples.Commands.DetachSampleProductFile;

/// <summary>
/// FluentValidation rules for <see cref="DetachSampleProductFileCommand"/>.
/// </summary>
public sealed class DetachSampleProductFileCommandValidator : AbstractValidator<DetachSampleProductFileCommand>
{
    /// <summary>
    /// Initializes validation rules for removing a file from a product.
    /// </summary>
    public DetachSampleProductFileCommandValidator()
    {
        RuleFor(x => x.SampleProductId)
            .NotEmpty()
            .WithMessage(ValidationCodes.IdRequired);

        RuleFor(x => x.FileId)
            .NotEmpty()
            .WithMessage(ValidationCodes.IdRequired);
    }
}
