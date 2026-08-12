namespace LogisticsAssetTracker.Api.Dtos.AuditLogs;

public class AuditLogListQuery
{
    public Guid? UserId { get; set; }
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? Action { get; set; }

    // Not in Document 06's base list, but Admin/Manager visibility is already backend-enforced
    // for this whole endpoint, and the approved Phase 4 approach sources the Suspicious Activity
    // page from this endpoint filtered to isSuspicious=true.
    public bool? IsSuspicious { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
}
