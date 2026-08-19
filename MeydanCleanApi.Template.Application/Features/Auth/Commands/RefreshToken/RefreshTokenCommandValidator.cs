using FluentValidation;
using MeydanCleanApi.Template.Domain.Constants;

namespace MeydanCleanApi.Template.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// FluentValidation rules for <see cref="RefreshTokenCommand"/>.
/// </summary>
/// <remarks>
/// The token itself is optional because browser clients send it in a cookie rather than the body.
/// Only its length is bounded here, to reject obviously malformed input early.
/// </remarks>
public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    /// <summary>
    /// Initializes validation rules for token refresh.
    /// </summary>
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .MaximumLength(ValidationConstants.MaxTokenLength)
            .WithMessage(ErrorCodes.ExternalAuthTokenInvalid)
            .When(x => x.RefreshToken is not null);
    }
}
