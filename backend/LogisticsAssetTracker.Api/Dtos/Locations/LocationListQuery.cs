using LogisticsAssetTracker.Api.Domain.Enums;

namespace LogisticsAssetTracker.Api.Dtos.Locations;

public class LocationListQuery
{
    public LocationType? Type { get; set; }
    public bool? IsActive { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
}
