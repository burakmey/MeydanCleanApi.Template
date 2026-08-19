using FluentValidation;
using MeydanCleanApi.Template.Domain.Constants;
using MeydanCleanApi.Template.Domain.Enums;

namespace MeydanCleanApi.Template.Application.Features.Auth.Commands.ExternalLogin;

/// <summary>
/// FluentValidation rules for <see cref="ExternalLoginCommand"/>.
/// </summary>
public sealed class ExternalLoginCommandValidator : AbstractValidator<ExternalLoginCommand>
{
    /// <summary>
    /// Initializes validation rules for external provider sign-in.
    /// </summary>
    public ExternalLoginCommandValidator()
    {
        // Rejects both undefined enum values and Local, which is email + password rather than a provider.
        RuleFor(x => x.AuthProvider)
            .Must(provider => Enum.IsDefined(provider) && provider != AuthProviderType.Local)
            .WithMessage(ErrorCodes.ExternalAuthProviderNotSupported);

        RuleFor(x => x.IdToken)
            .NotEmpty()
            .WithMessage(ErrorCodes.ExternalAuthTokenRequired);
    }
}
