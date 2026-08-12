using LogisticsAssetTracker.Api.Dtos.Approvals;
using LogisticsAssetTracker.Api.Dtos.Assets;

namespace LogisticsAssetTracker.Api.Dtos.Movements;

// ResultType is "MovementCreated" (Movement/Asset populated) or "ApprovalRequired"
// (Approval populated, AssetUnchanged = true, Movement/Asset stay null).
public class MovementResult
{
    public string ResultType { get; set; } = "MovementCreated";
    public AssetMovementResponse? Movement { get; set; }
    public AssetResponse? Asset { get; set; }
    public MovementApprovalResponse? Approval { get; set; }
    public bool? AssetUnchanged { get; set; }
}
