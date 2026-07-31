using LogisticsAssetTracker.Api.Domain.Enums;

namespace LogisticsAssetTracker.Api.Dtos.Assets;

public class CreateAssetRequest
{
    public string Name { get; set; } = string.Empty;
    public AssetType Type { get; set; }
    public Guid CurrentLocationId { get; set; }
    public AssetStatus Status { get; set; }
    public AssetCondition Condition { get; set; }
    public Guid? AssignedToUserId { get; set; }
}
