using FluentValidation;
using JaCore.Api.DTOs.Auth;

namespace JaCore.Api.DTOs.Auth.Validation;

public class TokenRefreshRequestDtoValidator : AbstractValidator<TokenRefreshRequestDto>
{
    public TokenRefreshRequestDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}