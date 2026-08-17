using LogisticsAssetTracker.Api.Domain.Enums;

namespace LogisticsAssetTracker.Api.Domain.Entities;

public class RiskRecommendation
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public Asset? Asset { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public RiskType RiskType { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public GenerationSource GenerationSource { get; set; }
    public string? ModelName { get; set; }
    public Guid? GeneratedByUserId { get; set; }
    public User? GeneratedByUser { get; set; }
    public DateTime CreatedAt { get; set; }
}
