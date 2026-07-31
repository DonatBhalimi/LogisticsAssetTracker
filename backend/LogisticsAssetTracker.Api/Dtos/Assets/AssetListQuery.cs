using LogisticsAssetTracker.Api.Domain.Enums;

namespace LogisticsAssetTracker.Api.Dtos.Assets;

public class AssetListQuery
{
    public AssetStatus? Status { get; set; }
    public AssetCondition? Condition { get; set; }
    public Guid? LocationId { get; set; }
    public AssetType? Type { get; set; }
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
}
