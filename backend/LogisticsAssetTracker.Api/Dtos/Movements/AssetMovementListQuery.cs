using LogisticsAssetTracker.Api.Domain.Enums;

namespace LogisticsAssetTracker.Api.Dtos.Movements;

public class AssetMovementListQuery
{
    public MovementSourceType? SourceType { get; set; }

    // Ignored for Operators: they must not use isSuspicious as a filter (Document 06).
    public bool? IsSuspicious { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
}
