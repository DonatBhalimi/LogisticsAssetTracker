using FluentValidation;
using LogisticsAssetTracker.Api.Dtos.Auth;

namespace LogisticsAssetTracker.Api.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
