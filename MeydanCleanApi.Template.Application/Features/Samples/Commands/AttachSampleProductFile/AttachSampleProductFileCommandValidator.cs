using FluentValidation;
using MeydanCleanApi.Template.Domain.Constants;
using MeydanCleanApi.Template.Domain.Enums;

namespace MeydanCleanApi.Template.Application.Features.Samples.Commands.AttachSampleProductFile;

/// <summary>
/// FluentValidation rules for <see cref="AttachSampleProductFileCommand"/>.
/// </summary>
public sealed class AttachSampleProductFileCommandValidator : AbstractValidator<AttachSampleProductFileCommand>
{
    /// <summary>
    /// Initializes validation rules for attaching a file to a product.
    /// </summary>
    public AttachSampleProductFileCommandValidator()
    {
        RuleFor(x => x.SampleProductId)
            .NotEmpty()
            .WithMessage(ValidationCodes.IdRequired);

        RuleFor(x => x.FileId)
            .NotEmpty()
            .WithMessage(ValidationCodes.IdRequired);

        // Rejects values outside the enum. Without this a caller could send 99 and the
        // foreign key to FilePurposes would fail at the database instead of here.
        RuleFor(x => x.Purpose)
            .Must(Enum.IsDefined)
            .WithMessage(ValidationCodes.FilePurposeInvalid);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage(ValidationCodes.SortOrderInvalid);
    }
}
