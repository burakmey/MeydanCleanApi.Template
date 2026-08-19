using FluentValidation;
using MeydanCleanApi.Template.Domain.Constants;

namespace MeydanCleanApi.Template.Application.Features.Files.Commands.ConfirmFileUpload;

/// <summary>
/// FluentValidation rules for <see cref="ConfirmFileUploadCommand"/>.
/// </summary>
public sealed class ConfirmFileUploadCommandValidator : AbstractValidator<ConfirmFileUploadCommand>
{
    /// <summary>
    /// Initializes validation rules for confirming an upload.
    /// </summary>
    public ConfirmFileUploadCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(ValidationCodes.IdRequired);
    }
}
