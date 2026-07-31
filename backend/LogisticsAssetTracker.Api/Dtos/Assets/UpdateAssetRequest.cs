using LogisticsAssetTracker.Api.Domain.Enums;

namespace LogisticsAssetTracker.Api.Dtos.Assets;

// Intentionally excludes currentLocationId, status, condition, assetCode, and qrCodeValue
// (Document 04 / Document 06 asset edit boundary — those change only via the movement flow).
public class UpdateAssetRequest
{
    public string Name { get; set; } = string.Empty;
    public AssetType Type { get; set; }
    public Guid? AssignedToUserId { get; set; }
}
