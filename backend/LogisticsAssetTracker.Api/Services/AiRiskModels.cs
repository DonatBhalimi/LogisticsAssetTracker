namespace LogisticsAssetTracker.Api.Services;

// Document 08 AI Input: structured data only, backend-provided, no invented fields.
public class AiRiskInput
{
    public string AssetCode { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public string CurrentCondition { get; set; } = string.Empty;
    public string CurrentLocation { get; set; } = string.Empty;
    public string RiskType { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;
    public double? HoursInTransit { get; set; }
    public double? DaysSinceUpdate { get; set; }
    public List<AiRecentMovement> RecentMovements { get; set; } = new();
}

public class AiRecentMovement
{
    public string FromLocation { get; set; } = string.Empty;
    public string ToLocation { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Document 08 Expected AI Output: reason + recommendation only. The backend owns
// riskType and riskLevel — AI never sees a field it could use to override them.
public class AiRiskExplanationResult
{
    public string Reason { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}
