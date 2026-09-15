using MeydanCleanApi.Template.Application.Constants.FileStorage;
using MeydanCleanApi.Template.Application.Features.Files.Commands.CreatePendingFile;
using MeydanCleanApi.Template.Domain.Constants;
using Xunit;

namespace MeydanCleanApi.Template.Tests.Application;

/// <summary>
/// Covers the rules that decide what may be staged for upload. Everything here arrives from the
/// client, so each rule is the only thing standing between a caller and the storage folder.
/// </summary>
public sealed class CreatePendingFileCommandValidatorTests
{
    private readonly CreatePendingFileCommandValidator _validator = new();

    private static CreatePendingFileCommand Command(
        string fileName = "photo.jpg",
        string contentType = "image/jpeg",
        long sizeInBytes = 1024,
        string container = FileStorageContainers.Temp.Default)
        => new(fileName, contentType, sizeInBytes, container);

    private static string[] CodesFor(FluentValidation.Results.ValidationResult result)
        => [.. result.Errors.Select(error => error.ErrorMessage)];

    [Fact]
    public void AWellFormedCommand_Passes()
    {
        Assert.True(_validator.Validate(Command()).IsValid);
    }

    [Theory]
    [InlineData("photo.jpg")]
    [InlineData("photo.JPG")]
    [InlineData("scan.pdf")]
    [InlineData("sheet.xlsx")]
    public void AnAllowedExtension_Passes(string fileName)
    {
        Assert.True(_validator.Validate(Command(fileName: fileName)).IsValid);
    }

    [Theory]
    [InlineData("payload.exe")]
    [InlineData("script.sh")]
    [InlineData("library.dll")]
    [InlineData("no-extension")]
    public void AnExtensionOffTheAllowList_IsRejected(string fileName)
    {
        var result = _validator.Validate(Command(fileName: fileName));

        Assert.False(result.IsValid);
        Assert.Contains(ValidationCodes.FileExtensionInvalid, CodesFor(result));
    }

    [Fact]
    public void AContainerOffTheWhitelist_IsRejected()
    {
        // Unchecked, this is what lets a caller aim an upload outside the storage folder.
        var result = _validator.Validate(Command(container: "../../etc"));

        Assert.False(result.IsValid);
        Assert.Contains(ValidationCodes.FileContainerInvalid, CodesFor(result));
    }

    [Theory]
    [InlineData(FileStorageContainers.Temp.Default)]
    [InlineData(FileStorageContainers.Public.Products)]
    [InlineData(FileStorageContainers.Private.Documents)]
    public void AWhitelistedContainer_Passes(string container)
    {
        Assert.True(_validator.Validate(Command(container: container)).IsValid);
    }

    [Fact]
    public void AFileLargerThanTheLimit_IsRejected()
    {
        var result = _validator.Validate(
            Command(sizeInBytes: CreatePendingFileCommandValidator.MaxFileSizeInBytes + 1));

        Assert.False(result.IsValid);
        Assert.Contains(ValidationCodes.FileSizeTooLarge, CodesFor(result));
    }

    [Fact]
    public void AFileExactlyAtTheLimit_Passes()
    {
        Assert.True(_validator.Validate(
            Command(sizeInBytes: CreatePendingFileCommandValidator.MaxFileSizeInBytes)).IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AnEmptyOrNegativeSize_IsRejected(long sizeInBytes)
    {
        var result = _validator.Validate(Command(sizeInBytes: sizeInBytes));

        Assert.False(result.IsValid);
        Assert.Contains(ValidationCodes.FileSizeInvalid, CodesFor(result));
    }

    [Fact]
    public void AMissingContentType_IsRejected()
    {
        var result = _validator.Validate(Command(contentType: string.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(ValidationCodes.FileContentTypeRequired, CodesFor(result));
    }

    [Fact]
    public void AMissingFileName_IsRejected()
    {
        var result = _validator.Validate(Command(fileName: string.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(ValidationCodes.FileNameRequired, CodesFor(result));
    }
}
