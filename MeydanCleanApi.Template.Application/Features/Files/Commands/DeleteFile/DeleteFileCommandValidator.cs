using FluentValidation;
using MeydanCleanApi.Template.Domain.Constants;

namespace MeydanCleanApi.Template.Application.Features.Files.Commands.DeleteFile;

/// <summary>
/// FluentValidation rules for <see cref="DeleteFileCommand"/>.
/// </summary>
public sealed class DeleteFileCommandValidator : AbstractValidator<DeleteFileCommand>
{
    /// <summary>
    /// Initializes validation rules for deleting a file.
    /// </summary>
    public DeleteFileCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(ValidationCodes.IdRequired);
    }
}
