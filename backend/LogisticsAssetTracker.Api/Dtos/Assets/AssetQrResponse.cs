namespace LogisticsAssetTracker.Api.Dtos.Assets;

public class AssetQrResponse
{
    public Guid AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string QrCodeValue { get; set; } = string.Empty;
    public string QrUrl { get; set; } = string.Empty;
}
