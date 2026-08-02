using LogisticsAssetTracker.Api.Domain.Enums;

namespace LogisticsAssetTracker.Api.Dtos.Movements;

// sourceType is intentionally absent: it is always backend-derived from the endpoint called
// (Document 05/06 — the client must not choose sourceType).
public class MovementRequest
{
    public Guid? ToLocationId { get; set; }
    public AssetStatus? NewStatus { get; set; }
    public AssetCondition? NewCondition { get; set; }
    public string? Notes { get; set; }
}
