using LogisticsAssetTracker.Api.Domain.Enums;

namespace LogisticsAssetTracker.Api.Dtos.Locations;

public class CreateLocationRequest
{
    public string Name { get; set; } = string.Empty;
    public LocationType Type { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
}
