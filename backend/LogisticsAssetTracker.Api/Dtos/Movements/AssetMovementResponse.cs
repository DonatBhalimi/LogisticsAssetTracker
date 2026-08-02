using LogisticsAssetTracker.Api.Domain.Enums;

namespace LogisticsAssetTracker.Api.Dtos.Movements;

public class AssetMovementResponse
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public Guid FromLocationId { get; set; }
    public string FromLocationName { get; set; } = string.Empty;
    public Guid ToLocationId { get; set; }
    public string ToLocationName { get; set; } = string.Empty;
    public AssetStatus PreviousStatus { get; set; }
    public AssetStatus NewStatus { get; set; }
    public AssetCondition PreviousCondition { get; set; }
    public AssetCondition NewCondition { get; set; }
    public MovementSourceType SourceType { get; set; }
    public Guid UpdatedByUserId { get; set; }
    public string UpdatedByUserName { get; set; } = string.Empty;
    public string? Notes { get; set; }

    // Null (not false) when hidden from the requester: Operators must not see suspicious
    // indicators or reasons (Document 06). Admin/Manager always get the real value.
    public bool? IsSuspicious { get; set; }
    public string? SuspiciousReason { get; set; }

    public DateTime CreatedAt { get; set; }
}
