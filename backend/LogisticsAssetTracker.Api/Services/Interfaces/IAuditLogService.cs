namespace LogisticsAssetTracker.Api.Services.Interfaces;

public interface IAuditLogService
{
    void Log(
        Guid? userId,
        string action,
        string entityType,
        Guid? entityId,
        object? metadata = null,
        bool isSuspicious = false,
        string? suspiciousReason = null);
}
