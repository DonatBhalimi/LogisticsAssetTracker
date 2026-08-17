using LogisticsAssetTracker.Api.Common;
using LogisticsAssetTracker.Api.Data;
using LogisticsAssetTracker.Api.Domain.Entities;
using LogisticsAssetTracker.Api.Domain.Enums;
using LogisticsAssetTracker.Api.Dtos.Dashboard;
using LogisticsAssetTracker.Api.Dtos.Movements;
using LogisticsAssetTracker.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LogisticsAssetTracker.Api.Services;

// Document 06 Dashboard Endpoints. riskyAssets and suspiciousActivityCount are computed
// live from current data (never from stored RiskRecommendation records — Document 06:
// "stored RiskRecommendation records do not control live counts"), reusing the same
// RiskRuleEngine that /risk/analyze uses so the two stay consistent with each other.
public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;
    private readonly RiskRuleEngine _ruleEngine;

    public DashboardService(AppDbContext db, RiskRuleEngine ruleEngine)
    {
        _db = db;
        _ruleEngine = ruleEngine;
    }

    public async Task<DashboardSummaryResponse> GetSummaryAsync()
    {
        var activeAssets = await _db.Assets.Where(a => a.IsActive).ToListAsync();

        var riskyAssets = await ComputeRiskyAssetCountAsync(activeAssets);
        var suspiciousActivityCount = await _db.AuditLogs.CountAsync(l => l.IsSuspicious);
        var pendingApprovals = await _db.MovementApprovals.CountAsync(a => a.ApprovalStatus == ApprovalStatus.Pending);

        return new DashboardSummaryResponse
        {
            TotalActiveAssets = activeAssets.Count,
            AvailableAssets = activeAssets.Count(a => a.Status == AssetStatus.Available),
            InTransitAssets = activeAssets.Count(a => a.Status == AssetStatus.InTransit),
            DamagedAssets = activeAssets.Count(a => a.Condition == AssetCondition.Damaged),
            LostAssets = activeAssets.Count(a => a.Status == AssetStatus.Lost),
            RiskyAssets = riskyAssets,
            SuspiciousActivityCount = suspiciousActivityCount,
            PendingApprovals = pendingApprovals
        };
    }

    public async Task<List<AssetGroupCount>> GetAssetsByStatusAsync()
    {
        return await _db.Assets
            .Where(a => a.IsActive)
            .GroupBy(a => a.Status)
            .Select(g => new AssetGroupCount { Key = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();
    }

    public async Task<List<AssetGroupCount>> GetAssetsByConditionAsync()
    {
        return await _db.Assets
            .Where(a => a.IsActive)
            .GroupBy(a => a.Condition)
            .Select(g => new AssetGroupCount { Key = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();
    }

    public async Task<List<AssetGroupCount>> GetAssetsByLocationAsync()
    {
        return await _db.Assets
            .Where(a => a.IsActive)
            .GroupBy(a => a.CurrentLocation!.Name)
            .Select(g => new AssetGroupCount { Key = g.Key, Count = g.Count() })
            .ToListAsync();
    }

    public async Task<PagedResult<AssetMovementResponse>> GetRecentMovementsAsync(AssetMovementListQuery query)
    {
        var movements = _db.AssetMovements
            .Include(m => m.FromLocation)
            .Include(m => m.ToLocation)
            .Include(m => m.UpdatedByUser)
            .AsQueryable();

        if (query.SourceType.HasValue)
        {
            movements = movements.Where(m => m.SourceType == query.SourceType.Value);
        }

        if (query.IsSuspicious.HasValue)
        {
            movements = movements.Where(m => m.IsSuspicious == query.IsSuspicious.Value);
        }

        if (query.FromDate.HasValue)
        {
            var fromDateUtc = DateTime.SpecifyKind(query.FromDate.Value.Date, DateTimeKind.Utc);
            movements = movements.Where(m => m.CreatedAt >= fromDateUtc);
        }

        if (query.ToDate.HasValue)
        {
            var toDateUtc = DateTime.SpecifyKind(query.ToDate.Value.Date, DateTimeKind.Utc).AddDays(1);
            movements = movements.Where(m => m.CreatedAt < toDateUtc);
        }

        if (query.UpdatedByUserId.HasValue)
        {
            movements = movements.Where(m => m.UpdatedByUserId == query.UpdatedByUserId.Value);
        }

        movements = query.SortDirection?.ToLowerInvariant() == "asc"
            ? movements.OrderBy(m => m.CreatedAt)
            : movements.OrderByDescending(m => m.CreatedAt);

        var totalCount = await movements.CountAsync();
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

        var items = await movements.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<AssetMovementResponse>
        {
            // Dashboard is Admin/Manager-only (Document 07): never Operator, so suspicious
            // fields are always visible — reuses the same mapper Phase 4 already established.
            Items = items.Select(m => AssetMovementService.MapMovementToResponse(m, UserRole.Manager)).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    private async Task<int> ComputeRiskyAssetCountAsync(List<Asset> activeAssets)
    {
        if (activeAssets.Count == 0)
        {
            return 0;
        }

        var assetIds = activeAssets.Select(a => a.Id).ToHashSet();

        var lastMovementByAsset = await _db.AssetMovements
            .Where(m => assetIds.Contains(m.AssetId))
            .GroupBy(m => m.AssetId)
            .Select(g => new { AssetId = g.Key, LastMovementAt = g.Max(m => m.CreatedAt) })
            .ToDictionaryAsync(x => x.AssetId, x => x.LastMovementAt);

        var suspiciousMovements = await _db.AssetMovements
            .Where(m => m.IsSuspicious && assetIds.Contains(m.AssetId))
            .Select(m => new { m.AssetId, m.SuspiciousReason })
            .ToListAsync();

        var suspiciousAuditEntries = await _db.AuditLogs
            .Where(l => l.IsSuspicious && l.MetadataJson != null)
            .Select(l => new { l.MetadataJson, l.SuspiciousReason })
            .ToListAsync();

        var suspiciousReasonsByAsset = new Dictionary<Guid, List<string?>>();

        foreach (var m in suspiciousMovements)
        {
            AddReason(suspiciousReasonsByAsset, m.AssetId, m.SuspiciousReason);
        }

        foreach (var entry in suspiciousAuditEntries)
        {
            var assetId = AuditMetadataHelper.TryGetAssetId(entry.MetadataJson);
            if (assetId.HasValue && assetIds.Contains(assetId.Value))
            {
                AddReason(suspiciousReasonsByAsset, assetId.Value, entry.SuspiciousReason);
            }
        }

        var riskyCount = 0;
        foreach (var asset in activeAssets)
        {
            var lastMovementAt = lastMovementByAsset.TryGetValue(asset.Id, out var lastMoved) ? lastMoved : asset.CreatedAt;
            var hasSuspicious = suspiciousReasonsByAsset.TryGetValue(asset.Id, out var reasons) && reasons.Count > 0;
            var escalates = hasSuspicious && reasons!.Any(r =>
                !string.IsNullOrEmpty(r) &&
                (r.Contains("Lost-to-Available", StringComparison.OrdinalIgnoreCase) ||
                 r.Contains("Repeated Lost-reactivation requests exceeded", StringComparison.OrdinalIgnoreCase)));

            var risks = _ruleEngine.DetectRisks(asset, lastMovementAt, hasSuspicious, escalates);
            if (risks.Count > 0)
            {
                riskyCount++;
            }
        }

        return riskyCount;
    }

    private static void AddReason(Dictionary<Guid, List<string?>> map, Guid assetId, string? reason)
    {
        if (!map.TryGetValue(assetId, out var list))
        {
            list = new List<string?>();
            map[assetId] = list;
        }

        list.Add(reason);
    }
}
