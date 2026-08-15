using System.Text.Json;
using LogisticsAssetTracker.Api.Common;
using LogisticsAssetTracker.Api.Data;
using LogisticsAssetTracker.Api.Domain.Entities;
using LogisticsAssetTracker.Api.Dtos.AuditLogs;
using LogisticsAssetTracker.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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

    public async Task<PagedResult<AuditLogResponse>> QueryAsync(AuditLogListQuery query)
    {
        var logs = _db.AuditLogs.Include(l => l.User).AsQueryable();

        if (query.UserId.HasValue)
        {
            logs = logs.Where(l => l.UserId == query.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityType))
        {
            logs = logs.Where(l => l.EntityType == query.EntityType);
        }

        if (query.EntityId.HasValue)
        {
            logs = logs.Where(l => l.EntityId == query.EntityId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            // Case-insensitive contains: a free-text search box, not a dropdown, so an
            // exact case-sensitive match is unusably strict.
            logs = logs.Where(l => EF.Functions.ILike(l.Action, $"%{query.Action}%"));
        }

        if (query.IsSuspicious.HasValue)
        {
            logs = logs.Where(l => l.IsSuspicious == query.IsSuspicious.Value);
        }

        if (query.FromDate.HasValue)
        {
            // Query-string-bound DateTime values come back with Kind=Unspecified; Npgsql
            // refuses to compare that against a timestamptz column, so it must be pinned
            // to UTC explicitly before use (the stored CreatedAt values are always UTC).
            var fromDateUtc = DateTime.SpecifyKind(query.FromDate.Value.Date, DateTimeKind.Utc);
            logs = logs.Where(l => l.CreatedAt >= fromDateUtc);
        }

        if (query.ToDate.HasValue)
        {
            // Inclusive of the entire "to" day, not just its midnight instant.
            var toDateUtc = DateTime.SpecifyKind(query.ToDate.Value.Date, DateTimeKind.Utc).AddDays(1);
            logs = logs.Where(l => l.CreatedAt < toDateUtc);
        }

        logs = query.SortDirection?.ToLowerInvariant() == "asc"
            ? logs.OrderBy(l => l.CreatedAt)
            : logs.OrderByDescending(l => l.CreatedAt);

        var totalCount = await logs.CountAsync();
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

        var items = await logs
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new AuditLogResponse
            {
                Id = l.Id,
                UserId = l.UserId,
                UserName = l.User != null ? l.User.FullName : null,
                Action = l.Action,
                EntityType = l.EntityType,
                EntityId = l.EntityId,
                MetadataJson = l.MetadataJson,
                CreatedAt = l.CreatedAt,
                IsSuspicious = l.IsSuspicious,
                SuspiciousReason = l.SuspiciousReason
            })
            .ToListAsync();

        return new PagedResult<AuditLogResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
