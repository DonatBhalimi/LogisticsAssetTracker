using FluentValidation;
using LogisticsAssetTracker.Api.Dtos.Assets;

namespace LogisticsAssetTracker.Api.Validators;

public class UpdateAssetRequestValidator : AbstractValidator<UpdateAssetRequest>
{
    public UpdateAssetRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).IsInEnum();
    }
}
