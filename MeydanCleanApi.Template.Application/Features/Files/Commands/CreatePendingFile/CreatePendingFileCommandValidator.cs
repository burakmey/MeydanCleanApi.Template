using FluentValidation;
using MeydanCleanApi.Template.Application.Constants.FileStorage;
using MeydanCleanApi.Template.Domain.Constants;

namespace MeydanCleanApi.Template.Application.Features.Files.Commands.CreatePendingFile;

/// <summary>
/// FluentValidation rules for <see cref="CreatePendingFileCommand"/>.
/// </summary>
/// <remarks>
/// Everything here arrives from the client, so all of it is checked. Two rules matter most:
/// the container must be one this API knows about, and the extension must be on the allow list.
/// Without them a caller could point uploads anywhere on disk or store an executable.
/// </remarks>
public sealed class CreatePendingFileCommandValidator : AbstractValidator<CreatePendingFileCommand>
{
    /// <summary>Largest upload accepted, in bytes (25 MB).</summary>
    public const long MaxFileSizeInBytes = 25L * 1024 * 1024;

    /// <summary>File extensions this API accepts.</summary>
    public static readonly string[] AllowedExtensions =
        [".jpg", ".jpeg", ".png", ".webp", ".gif", ".pdf", ".docx", ".xlsx", ".csv", ".txt"];

    /// <summary>
    /// Initializes validation rules for staging a file upload.
    /// </summary>
    public CreatePendingFileCommandValidator()
    {
        RuleFor(x => x.OriginalFileName)
            .NotEmpty()
            .WithMessage(ValidationCodes.FileNameRequired)
            .MaximumLength(ValidationConstants.MaxFileNameLength)
            .WithMessage(ValidationCodes.FileNameRequired);

        RuleFor(x => x.OriginalFileName)
            .Must(HasAllowedExtension)
            .WithMessage(ValidationCodes.FileExtensionInvalid)
            .When(x => !string.IsNullOrWhiteSpace(x.OriginalFileName));

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithMessage(ValidationCodes.FileContentTypeRequired)
            .MaximumLength(ValidationConstants.MaxTitleLength)
            .WithMessage(ValidationCodes.FileContentTypeRequired);

        RuleFor(x => x.SizeInBytes)
            .GreaterThan(0)
            .WithMessage(ValidationCodes.FileSizeInvalid)
            .LessThanOrEqualTo(MaxFileSizeInBytes)
            .WithMessage(ValidationCodes.FileSizeTooLarge);

        RuleFor(x => x.Container)
            .Must(FileStorageContainers.IsAllowed)
            .WithMessage(ValidationCodes.FileContainerInvalid);
    }

    private static bool HasAllowedExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        return !string.IsNullOrWhiteSpace(extension)
            && AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }
}
