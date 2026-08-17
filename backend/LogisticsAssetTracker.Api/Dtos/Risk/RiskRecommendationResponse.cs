using LogisticsAssetTracker.Api.Domain.Enums;

namespace LogisticsAssetTracker.Api.Dtos.Risk;

public class RiskRecommendationResponse
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public RiskLevel RiskLevel { get; set; }
    public RiskType RiskType { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public GenerationSource GenerationSource { get; set; }
    public string? ModelName { get; set; }
    public Guid? GeneratedByUserId { get; set; }
    public string? GeneratedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
}
