using FluentValidation;
using LogisticsAssetTracker.Api.Dtos.Movements;

namespace LogisticsAssetTracker.Api.Validators;

// Basic request-shape validation only. Business rules (active/retired checks, no-change
// rejection, Phase 3 status/condition restriction) are enforced in AssetMovementService.
public class MovementRequestValidator : AbstractValidator<MovementRequest>
{
    public MovementRequestValidator()
    {
        RuleFor(x => x.NewStatus).IsInEnum().When(x => x.NewStatus.HasValue);
        RuleFor(x => x.NewCondition).IsInEnum().When(x => x.NewCondition.HasValue);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}
