using LogisticsAssetTracker.Api.Domain.Enums;

namespace LogisticsAssetTracker.Api.Dtos.Locations;

public class LocationResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public LocationType Type { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
