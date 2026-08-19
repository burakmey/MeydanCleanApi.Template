using FluentValidation;
using MeydanCleanApi.Template.Domain.Constants;

namespace MeydanCleanApi.Template.Application.Features.Auth.Commands.Login;

/// <summary>
/// FluentValidation rules for <see cref="LoginCommand"/>.
/// </summary>
/// <remarks>
/// Only shape is checked here, never whether the credentials are correct. Telling a caller that the
/// email exists but the password is wrong would help somebody guessing accounts.
/// </remarks>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    /// <summary>
    /// Initializes validation rules for the login command.
    /// </summary>
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(ValidationCodes.EmailRequired)
            .EmailAddress()
            .WithMessage(ValidationCodes.EmailInvalid)
            .MaximumLength(ValidationConstants.MaxEmailLength)
            .WithMessage(ValidationCodes.EmailInvalid);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(ValidationCodes.PasswordRequired);
    }
}
