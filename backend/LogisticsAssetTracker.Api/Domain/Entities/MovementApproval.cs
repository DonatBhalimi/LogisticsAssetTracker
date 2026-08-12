using LogisticsAssetTracker.Api.Domain.Enums;

namespace LogisticsAssetTracker.Api.Domain.Entities;

public class MovementApproval
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public Asset? Asset { get; set; }
    public Guid RequestedByUserId { get; set; }
    public User? RequestedByUser { get; set; }
    public Guid? DecidedByUserId { get; set; }
    public User? DecidedByUser { get; set; }
    public Guid? RequestedLocationId { get; set; }
    public Location? RequestedLocation { get; set; }
    public AssetStatus RequestedStatus { get; set; }
    public AssetCondition RequestedCondition { get; set; }
    public MovementSourceType RequestedSourceType { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; }
    public string RequestReason { get; set; } = string.Empty;
    public string? DecisionNote { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DecidedAt { get; set; }
}
