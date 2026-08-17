using LogisticsAssetTracker.Api.Domain.Enums;

namespace LogisticsAssetTracker.Api.Dtos.Risk;

public class RiskRecommendationListQuery
{
    public Guid? AssetId { get; set; }
    public RiskLevel? RiskLevel { get; set; }
    public RiskType? RiskType { get; set; }
    public GenerationSource? GenerationSource { get; set; }
    public Guid? GeneratedByUserId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
}
