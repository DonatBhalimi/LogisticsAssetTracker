namespace LogisticsAssetTracker.Api.Dtos.Risk;

// Document 08 flow step 2: if no risk is detected, no RiskRecommendation is created —
// HasRisk = false and Recommendations stays empty rather than an error.
public class RiskAnalyzeResponse
{
    public bool HasRisk { get; set; }
    public List<RiskRecommendationResponse> Recommendations { get; set; } = new();
}
