using LogisticsAssetTracker.Api.Domain.Enums;

namespace LogisticsAssetTracker.Api.Dtos.Approvals;

public class MovementApprovalResponse
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public Guid RequestedByUserId { get; set; }
    public string RequestedByUserName { get; set; } = string.Empty;
    public Guid? DecidedByUserId { get; set; }
    public string? DecidedByUserName { get; set; }
    public Guid? RequestedLocationId { get; set; }
    public string? RequestedLocationName { get; set; }
    public AssetStatus RequestedStatus { get; set; }
    public AssetCondition RequestedCondition { get; set; }
    public MovementSourceType RequestedSourceType { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; }
    public string RequestReason { get; set; } = string.Empty;
    public string? DecisionNote { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DecidedAt { get; set; }
}
