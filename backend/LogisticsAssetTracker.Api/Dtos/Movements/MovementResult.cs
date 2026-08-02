using LogisticsAssetTracker.Api.Dtos.Assets;

namespace LogisticsAssetTracker.Api.Dtos.Movements;

public class MovementResult
{
    public string ResultType { get; set; } = "MovementCreated";
    public AssetMovementResponse Movement { get; set; } = null!;
    public AssetResponse Asset { get; set; } = null!;
}
