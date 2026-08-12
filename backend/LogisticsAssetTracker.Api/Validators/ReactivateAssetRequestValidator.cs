using FluentValidation;
using LogisticsAssetTracker.Api.Dtos.Assets;

namespace LogisticsAssetTracker.Api.Validators;

// Document 05/06: direct Lost reactivation requires a non-empty decision note.
public class ReactivateAssetRequestValidator : AbstractValidator<ReactivateAssetRequest>
{
    public ReactivateAssetRequestValidator()
    {
        RuleFor(x => x.DecisionNote).NotEmpty().MaximumLength(2000);
    }
}
