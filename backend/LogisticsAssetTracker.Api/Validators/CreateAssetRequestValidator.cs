using FluentValidation;
using LogisticsAssetTracker.Api.Dtos.Assets;

namespace LogisticsAssetTracker.Api.Validators;

// Basic request-shape validation only (Document 03: controllers/validators check shape,
// services enforce business rules). Creation-specific business rules (no direct Retired,
// Available requires Good) are enforced in AssetService and return 422, not 400.
public class CreateAssetRequestValidator : AbstractValidator<CreateAssetRequest>
{
    public CreateAssetRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.CurrentLocationId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Condition).IsInEnum();
    }
}
