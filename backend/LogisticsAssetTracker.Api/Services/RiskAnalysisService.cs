using LogisticsAssetTracker.Api.Common;
using LogisticsAssetTracker.Api.Data;
using LogisticsAssetTracker.Api.Domain.Entities;
using LogisticsAssetTracker.Api.Domain.Enums;
using LogisticsAssetTracker.Api.Dtos.Risk;
using LogisticsAssetTracker.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LogisticsAssetTracker.Api.Services;

// Document 08 AI Recommendation Flow: rule-based detection first, AI only explains,
// backend owns riskType/riskLevel, deterministic fallback when AI output is missing
// or invalid, one RiskRecommendation per detected risk type, none created when no risk.
public class RiskAnalysisService : IRiskAnalysisService
{
    private readonly AppDbContext _db;
    private readonly IAuditLogService _auditLog;
    private readonly RiskRuleEngine _ruleEngine;
    private readonly IAiRiskExplanationService _aiService;

    public RiskAnalysisService(AppDbContext db, IAuditLogService auditLog, RiskRuleEngine ruleEngine, IAiRiskExplanationService aiService)
    {
        _db = db;
        _auditLog = auditLog;
        _ruleEngine = ruleEngine;
        _aiService = aiService;
    }

    public async Task<RiskAnalyzeResponse> AnalyzeAsync(Guid assetId, Guid actingUserId)
    {
        var asset = await _db.Assets.Include(a => a.CurrentLocation).FirstOrDefaultAsync(a => a.Id == assetId)
            ?? throw ApiException.NotFound("ASSET_NOT_FOUND", "Asset not found.");

        var lastMovementAt = await _db.AssetMovements
            .Where(m => m.AssetId == assetId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => (DateTime?)m.CreatedAt)
            .FirstOrDefaultAsync() ?? asset.CreatedAt;

        var (hasSuspicious, escalates) = await GetSuspiciousInfoAsync(assetId);

        var detectedRisks = _ruleEngine.DetectRisks(asset, lastMovementAt, hasSuspicious, escalates);

        if (detectedRisks.Count == 0)
        {
            return new RiskAnalyzeResponse { HasRisk = false, Recommendations = new List<RiskRecommendationResponse>() };
        }

        var recentMovements = await _db.AssetMovements
            .Include(m => m.FromLocation)
            .Include(m => m.ToLocation)
            .Where(m => m.AssetId == assetId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(5)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var hoursInTransit = asset.Status == AssetStatus.InTransit ? (now - lastMovementAt).TotalHours : (double?)null;
        var daysSinceUpdate = (now - lastMovementAt).TotalDays;
        var actingUser = await _db.Users.FindAsync(actingUserId);

        var created = new List<RiskRecommendation>();

        foreach (var risk in detectedRisks)
        {
            var aiInput = new AiRiskInput
            {
                AssetCode = asset.AssetCode,
                AssetType = asset.Type.ToString(),
                CurrentStatus = asset.Status.ToString(),
                CurrentCondition = asset.Condition.ToString(),
                CurrentLocation = asset.CurrentLocation?.Name ?? string.Empty,
                RiskType = risk.RiskType.ToString(),
                RiskLevel = risk.RiskLevel.ToString(),
                HoursInTransit = risk.RiskType == RiskType.DelayRisk ? hoursInTransit : null,
                DaysSinceUpdate = risk.RiskType == RiskType.NoRecentUpdate ? daysSinceUpdate : null,
                RecentMovements = recentMovements.Select(m => new AiRecentMovement
                {
                    FromLocation = m.FromLocation?.Name ?? string.Empty,
                    ToLocation = m.ToLocation?.Name ?? string.Empty,
                    NewStatus = m.NewStatus.ToString(),
                    SourceType = m.SourceType.ToString(),
                    CreatedAt = m.CreatedAt
                }).ToList()
            };

            AiRiskExplanationResult? aiResult;
            try
            {
                aiResult = await _aiService.ExplainAsync(aiInput);
            }
            catch
            {
                // AI must not fail the request (Document 08 Fallback Behavior).
                aiResult = null;
            }

            string reason;
            string recommendationText;
            GenerationSource source;
            string? modelName;

            if (IsValidAiOutput(aiResult))
            {
                reason = aiResult!.Reason.Trim();
                recommendationText = aiResult.Recommendation.Trim();
                source = GenerationSource.RuleEngineWithAI;
                modelName = _aiService.ModelName;
            }
            else
            {
                reason = $"This asset has been flagged as {risk.RiskType} risk based on system rules. {risk.Detail}";
                recommendationText = "Review its latest movement history and take the appropriate operational action.";
                source = GenerationSource.RuleEngine;
                modelName = null;
            }

            var entity = new RiskRecommendation
            {
                Id = Guid.NewGuid(),
                AssetId = asset.Id,
                RiskLevel = risk.RiskLevel,
                RiskType = risk.RiskType,
                Reason = reason,
                Recommendation = recommendationText,
                GenerationSource = source,
                ModelName = modelName,
                GeneratedByUserId = actingUserId,
                GeneratedByUser = actingUser,
                CreatedAt = DateTime.UtcNow
            };

            _db.RiskRecommendations.Add(entity);
            created.Add(entity);
        }

        _auditLog.Log(actingUserId, AuditActions.RiskRecommendationGenerated, AuditEntityTypes.Asset, asset.Id,
            new { AssetId = asset.Id, RiskTypes = detectedRisks.Select(r => r.RiskType.ToString()).ToList() });

        await _db.SaveChangesAsync();

        return new RiskAnalyzeResponse
        {
            HasRisk = true,
            Recommendations = created.Select(r => MapToResponse(r, asset)).ToList()
        };
    }

    public async Task<PagedResult<RiskRecommendationResponse>> GetForAssetAsync(Guid assetId, RiskRecommendationListQuery query)
    {
        query.AssetId = assetId;
        return await QueryAsync(query);
    }

    public async Task<PagedResult<RiskRecommendationResponse>> ListAsync(RiskRecommendationListQuery query)
    {
        return await QueryAsync(query);
    }

    private async Task<PagedResult<RiskRecommendationResponse>> QueryAsync(RiskRecommendationListQuery query)
    {
        var recommendations = _db.RiskRecommendations
            .Include(r => r.Asset)
            .Include(r => r.GeneratedByUser)
            .AsQueryable();

        if (query.AssetId.HasValue)
        {
            recommendations = recommendations.Where(r => r.AssetId == query.AssetId.Value);
        }

        if (query.RiskLevel.HasValue)
        {
            recommendations = recommendations.Where(r => r.RiskLevel == query.RiskLevel.Value);
        }

        if (query.RiskType.HasValue)
        {
            recommendations = recommendations.Where(r => r.RiskType == query.RiskType.Value);
        }

        if (query.GenerationSource.HasValue)
        {
            recommendations = recommendations.Where(r => r.GenerationSource == query.GenerationSource.Value);
        }

        if (query.GeneratedByUserId.HasValue)
        {
            recommendations = recommendations.Where(r => r.GeneratedByUserId == query.GeneratedByUserId.Value);
        }

        if (query.FromDate.HasValue)
        {
            var fromDateUtc = DateTime.SpecifyKind(query.FromDate.Value.Date, DateTimeKind.Utc);
            recommendations = recommendations.Where(r => r.CreatedAt >= fromDateUtc);
        }

        if (query.ToDate.HasValue)
        {
            var toDateUtc = DateTime.SpecifyKind(query.ToDate.Value.Date, DateTimeKind.Utc).AddDays(1);
            recommendations = recommendations.Where(r => r.CreatedAt < toDateUtc);
        }

        recommendations = query.SortDirection?.ToLowerInvariant() == "asc"
            ? recommendations.OrderBy(r => r.CreatedAt)
            : recommendations.OrderByDescending(r => r.CreatedAt);

        var totalCount = await recommendations.CountAsync();
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

        var items = await recommendations
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<RiskRecommendationResponse>
        {
            Items = items.Select(r => MapToResponse(r, r.Asset!)).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    private async Task<(bool HasSuspicious, bool Escalates)> GetSuspiciousInfoAsync(Guid assetId)
    {
        var movementReasons = await _db.AssetMovements
            .Where(m => m.AssetId == assetId && m.IsSuspicious)
            .Select(m => m.SuspiciousReason)
            .ToListAsync();

        var auditEntries = await _db.AuditLogs
            .Where(l => l.IsSuspicious && l.MetadataJson != null)
            .Select(l => new { l.MetadataJson, l.SuspiciousReason })
            .ToListAsync();

        var auditReasons = auditEntries
            .Where(e => AuditMetadataHelper.TryGetAssetId(e.MetadataJson) == assetId)
            .Select(e => e.SuspiciousReason)
            .ToList();

        var hasSuspicious = movementReasons.Count > 0 || auditReasons.Count > 0;

        var escalates = movementReasons.Concat(auditReasons)
            .Any(r => !string.IsNullOrEmpty(r) &&
                (r.Contains("Lost-to-Available", StringComparison.OrdinalIgnoreCase) ||
                 r.Contains("Repeated Lost-reactivation requests exceeded", StringComparison.OrdinalIgnoreCase)));

        return (hasSuspicious, escalates);
    }

    // Document 08 Validation Rules: valid AI output must be non-empty and reasonably
    // sized. AI cannot change riskType/riskLevel since AiRiskExplanationResult has no
    // such fields — that boundary is structural, not just a validation check.
    private static bool IsValidAiOutput(AiRiskExplanationResult? result)
    {
        if (result is null)
        {
            return false;
        }

        var reason = result.Reason?.Trim();
        var recommendation = result.Recommendation?.Trim();

        if (string.IsNullOrWhiteSpace(reason) || string.IsNullOrWhiteSpace(recommendation))
        {
            return false;
        }

        return reason.Length is >= 10 and <= 1000 && recommendation.Length is >= 10 and <= 1000;
    }

    private static RiskRecommendationResponse MapToResponse(RiskRecommendation entity, Asset asset) => new()
    {
        Id = entity.Id,
        AssetId = entity.AssetId,
        AssetCode = asset.AssetCode,
        AssetName = asset.Name,
        RiskLevel = entity.RiskLevel,
        RiskType = entity.RiskType,
        Reason = entity.Reason,
        Recommendation = entity.Recommendation,
        GenerationSource = entity.GenerationSource,
        ModelName = entity.ModelName,
        GeneratedByUserId = entity.GeneratedByUserId,
        GeneratedByUserName = entity.GeneratedByUser?.FullName,
        CreatedAt = entity.CreatedAt
    };
}
