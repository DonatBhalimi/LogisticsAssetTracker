using LogisticsAssetTracker.Api.Common;
using LogisticsAssetTracker.Api.Domain.Entities;
using LogisticsAssetTracker.Api.Domain.Enums;
using Microsoft.Extensions.Options;

namespace LogisticsAssetTracker.Api.Services;

public record DetectedRisk(RiskType RiskType, RiskLevel RiskLevel, string Detail);

// Document 05 rule-based risk detection. Pure computation over already-loaded data — no
// DB access, no AI — so RiskAnalysisService (persists + explains one asset) and
// DashboardService (live counts across all assets) share identical thresholds and logic.
public class RiskRuleEngine
{
    private readonly MovementThresholdsOptions _thresholds;

    public RiskRuleEngine(IOptions<MovementThresholdsOptions> thresholds)
    {
        _thresholds = thresholds.Value;
    }

    public List<DetectedRisk> DetectRisks(Asset asset, DateTime lastMovementAt, bool hasSuspiciousRecord, bool suspiciousEscalatesToCritical)
    {
        var risks = new List<DetectedRisk>();
        var now = DateTime.UtcNow;

        // DelayRisk and NoRecentUpdate apply only to active, non-Retired assets (Document 05 Risk Eligibility).
        var eligibleForTimeBasedRisk = asset.IsActive && asset.Status != AssetStatus.Retired;

        if (eligibleForTimeBasedRisk && asset.Status == AssetStatus.InTransit)
        {
            var hoursInTransit = (now - lastMovementAt).TotalHours;
            if (hoursInTransit > _thresholds.HighDelayRiskDays * 24)
            {
                risks.Add(new DetectedRisk(RiskType.DelayRisk, RiskLevel.High, $"InTransit for {hoursInTransit:F0} hours."));
            }
            else if (hoursInTransit > _thresholds.MaxInTransitHours)
            {
                risks.Add(new DetectedRisk(RiskType.DelayRisk, RiskLevel.Medium, $"InTransit for {hoursInTransit:F0} hours."));
            }
        }

        if (eligibleForTimeBasedRisk)
        {
            var daysSinceUpdate = (now - lastMovementAt).TotalDays;
            if (daysSinceUpdate >= _thresholds.HighStaleDays)
            {
                risks.Add(new DetectedRisk(RiskType.NoRecentUpdate, RiskLevel.High, $"No movement update for {daysSinceUpdate:F0} days."));
            }
            else if (daysSinceUpdate >= _thresholds.StaleAssetDays)
            {
                risks.Add(new DetectedRisk(RiskType.NoRecentUpdate, RiskLevel.Medium, $"No movement update for {daysSinceUpdate:F0} days."));
            }
        }

        if (asset.Condition == AssetCondition.Damaged)
        {
            risks.Add(new DetectedRisk(RiskType.DamagedAsset, RiskLevel.High, "Condition is Damaged."));
        }
        else if (asset.Condition == AssetCondition.NeedsInspection)
        {
            risks.Add(new DetectedRisk(RiskType.DamagedAsset, RiskLevel.Medium, "Condition is NeedsInspection."));
        }

        if (asset.Status == AssetStatus.Lost)
        {
            risks.Add(new DetectedRisk(RiskType.LostAsset, RiskLevel.Critical, "Status is Lost."));
        }

        if (hasSuspiciousRecord)
        {
            var level = suspiciousEscalatesToCritical ? RiskLevel.Critical : RiskLevel.High;
            risks.Add(new DetectedRisk(RiskType.SuspiciousMovement, level, "Asset has suspicious activity on record."));
        }

        return risks;
    }
}
