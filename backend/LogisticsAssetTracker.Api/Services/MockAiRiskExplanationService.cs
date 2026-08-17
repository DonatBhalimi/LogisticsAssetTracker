using LogisticsAssetTracker.Api.Services.Interfaces;

namespace LogisticsAssetTracker.Api.Services;

// Document 08: "Use mocked AI first." Deterministic, templated explanation generated
// in-process from structured input — no external API call, no API key, no network
// dependency. AI never receives or returns riskType/riskLevel; the backend owns those.
public class MockAiRiskExplanationService : IAiRiskExplanationService
{
    public string ModelName => "mock-risk-explainer-v1";

    public Task<AiRiskExplanationResult?> ExplainAsync(AiRiskInput input)
    {
        string reason;
        string recommendation;

        switch (input.RiskType)
        {
            case "DelayRisk":
                var hours = input.HoursInTransit.HasValue ? $"{input.HoursInTransit:F0} hours" : "an extended period";
                reason = $"{input.AssetCode} has been InTransit for {hours}, longer than expected for a {input.AssetType.ToLowerInvariant()}.";
                recommendation = $"Contact the operator handling {input.AssetCode} or the destination location ({input.CurrentLocation}) to confirm its current status and expected arrival.";
                break;

            case "NoRecentUpdate":
                var days = input.DaysSinceUpdate.HasValue ? $"{input.DaysSinceUpdate:F0} days" : "an extended period";
                reason = $"{input.AssetCode} has not received a movement update in {days}, which may indicate it is idle or untracked.";
                recommendation = $"Verify the physical location of {input.AssetCode} and record a movement update to confirm it is still accounted for.";
                break;

            case "DamagedAsset":
                reason = $"{input.AssetCode} is currently recorded with condition {input.CurrentCondition}, which affects its usability.";
                recommendation = input.CurrentCondition == "Damaged"
                    ? $"Route {input.AssetCode} for inspection or repair before it is used for further transit."
                    : $"Schedule an inspection for {input.AssetCode} so a Manager or Admin can clear it back to Good condition.";
                break;

            case "LostAsset":
                reason = $"{input.AssetCode} is currently marked Lost and is not available for normal operations.";
                recommendation = $"Investigate the last known location of {input.AssetCode} from its movement history, or process a Lost-reactivation request once it is located.";
                break;

            case "SuspiciousMovement":
                reason = $"{input.AssetCode} has one or more suspicious movement events on record, indicating unusual or blocked activity.";
                recommendation = $"Review the Suspicious Activity and Audit Logs pages for {input.AssetCode} to determine whether further action is required.";
                break;

            default:
                return Task.FromResult<AiRiskExplanationResult?>(null);
        }

        return Task.FromResult<AiRiskExplanationResult?>(new AiRiskExplanationResult { Reason = reason, Recommendation = recommendation });
    }
}
