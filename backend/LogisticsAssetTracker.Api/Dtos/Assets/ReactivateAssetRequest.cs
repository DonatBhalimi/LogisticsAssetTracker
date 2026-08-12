namespace LogisticsAssetTracker.Api.Dtos.Assets;

// Manager/Admin direct Lost-to-Available reactivation (Document 06). sourceType is not
// accepted here: the dedicated reactivation flow always produces sourceType Manual.
public class ReactivateAssetRequest
{
    public Guid? ToLocationId { get; set; }
    public string DecisionNote { get; set; } = string.Empty;
}
