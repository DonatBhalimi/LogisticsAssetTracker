using LogisticsAssetTracker.Api.Common;
using LogisticsAssetTracker.Api.Dtos.Risk;

namespace LogisticsAssetTracker.Api.Services.Interfaces;

public interface IRiskAnalysisService
{
    Task<RiskAnalyzeResponse> AnalyzeAsync(Guid assetId, Guid actingUserId);

    Task<PagedResult<RiskRecommendationResponse>> GetForAssetAsync(Guid assetId, RiskRecommendationListQuery query);

    Task<PagedResult<RiskRecommendationResponse>> ListAsync(RiskRecommendationListQuery query);
}
