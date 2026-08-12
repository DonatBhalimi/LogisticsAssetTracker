using FluentValidation;
using LogisticsAssetTracker.Api.Dtos.Approvals;

namespace LogisticsAssetTracker.Api.Validators;

// Document 05: approval and rejection decisions require a non-empty decision note.
public class ApprovalDecisionRequestValidator : AbstractValidator<ApprovalDecisionRequest>
{
    public ApprovalDecisionRequestValidator()
    {
        RuleFor(x => x.DecisionNote).NotEmpty().MaximumLength(2000);
    }
}
