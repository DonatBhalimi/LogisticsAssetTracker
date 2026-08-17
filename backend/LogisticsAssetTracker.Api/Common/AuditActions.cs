namespace LogisticsAssetTracker.Api.Common;

public static class AuditActions
{
    public const string UserCreated = "UserCreated";
    public const string UserUpdated = "UserUpdated";
    public const string LocationCreated = "LocationCreated";
    public const string LocationUpdated = "LocationUpdated";
    public const string LocationDeactivated = "LocationDeactivated";
    public const string AssetCreated = "AssetCreated";
    public const string AssetUpdated = "AssetUpdated";
    public const string AssetDeactivated = "AssetDeactivated";
    public const string MovementCreated = "MovementCreated";
    public const string MovementBlocked = "MovementBlocked";
    public const string MovementApprovalRequested = "MovementApprovalRequested";
    public const string MovementApprovalDuplicateRejected = "MovementApprovalDuplicateRejected";
    public const string MovementApprovalApproved = "MovementApprovalApproved";
    public const string MovementApprovalRejected = "MovementApprovalRejected";
    public const string AssetLostReactivated = "AssetLostReactivated";
    public const string AssetLostToRetired = "AssetLostToRetired";
    public const string AssetConditionCleared = "AssetConditionCleared";
    public const string RiskRecommendationGenerated = "RiskRecommendationGenerated";
}
