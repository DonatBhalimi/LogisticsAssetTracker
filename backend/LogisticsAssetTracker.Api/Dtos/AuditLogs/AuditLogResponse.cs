namespace LogisticsAssetTracker.Api.Dtos.AuditLogs;

public class AuditLogResponse
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? MetadataJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsSuspicious { get; set; }
    public string? SuspiciousReason { get; set; }
}
