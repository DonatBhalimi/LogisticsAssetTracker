using LogisticsAssetTracker.Api.Domain.Enums;

namespace LogisticsAssetTracker.Api.Domain.Entities;

public class AssetMovement
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public Asset? Asset { get; set; }
    public Guid FromLocationId { get; set; }
    public Location? FromLocation { get; set; }
    public Guid ToLocationId { get; set; }
    public Location? ToLocation { get; set; }
    public AssetStatus PreviousStatus { get; set; }
    public AssetStatus NewStatus { get; set; }
    public AssetCondition PreviousCondition { get; set; }
    public AssetCondition NewCondition { get; set; }
    public MovementSourceType SourceType { get; set; }
    public Guid UpdatedByUserId { get; set; }
    public User? UpdatedByUser { get; set; }
    public string? Notes { get; set; }
    public bool IsSuspicious { get; set; }
    public string? SuspiciousReason { get; set; }
    public DateTime CreatedAt { get; set; }
}
