using System.Text.Json;
using LogisticsAssetTracker.Api.Data;
using LogisticsAssetTracker.Api.Domain.Entities;
using LogisticsAssetTracker.Api.Services.Interfaces;

namespace LogisticsAssetTracker.Api.Services;

public class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _db;

    public AuditLogService(AppDbContext db)
    {
        _db = db;
    }

    public void Log(
        Guid? userId,
        string action,
        string entityType,
        Guid? entityId,
        object? metadata = null,
        bool isSuspicious = false,
        string? suspiciousReason = null)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            MetadataJson = metadata is null ? null : JsonSerializer.Serialize(metadata),
            CreatedAt = DateTime.UtcNow,
            IsSuspicious = isSuspicious,
            SuspiciousReason = suspiciousReason
        });
    }
}
