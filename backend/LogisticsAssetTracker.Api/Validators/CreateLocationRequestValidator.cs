using FluentValidation;
using LogisticsAssetTracker.Api.Dtos.Locations;

namespace LogisticsAssetTracker.Api.Validators;

public class CreateLocationRequestValidator : AbstractValidator<CreateLocationRequest>
{
    public CreateLocationRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Address).MaximumLength(300);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}
