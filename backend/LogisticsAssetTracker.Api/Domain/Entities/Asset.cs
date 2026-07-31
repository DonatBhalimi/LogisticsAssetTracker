using LogisticsAssetTracker.Api.Domain.Enums;

namespace LogisticsAssetTracker.Api.Domain.Entities;

public class Asset
{
    public Guid Id { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string NormalizedAssetCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AssetType Type { get; set; }
    public Guid CurrentLocationId { get; set; }
    public Location? CurrentLocation { get; set; }
    public AssetStatus Status { get; set; }
    public AssetCondition Condition { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }
    public string QrCodeValue { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
