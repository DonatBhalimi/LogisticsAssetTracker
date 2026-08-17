using LogisticsAssetTracker.Api.Services;

namespace LogisticsAssetTracker.Api.Services.Interfaces;

public interface IAiRiskExplanationService
{
    string ModelName { get; }

    Task<AiRiskExplanationResult?> ExplainAsync(AiRiskInput input);
}
