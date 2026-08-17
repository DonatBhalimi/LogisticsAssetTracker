using LogisticsAssetTracker.Api.Common;
using LogisticsAssetTracker.Api.Dtos.Dashboard;
using LogisticsAssetTracker.Api.Dtos.Movements;

namespace LogisticsAssetTracker.Api.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryResponse> GetSummaryAsync();

    Task<List<AssetGroupCount>> GetAssetsByStatusAsync();

    Task<List<AssetGroupCount>> GetAssetsByConditionAsync();

    Task<List<AssetGroupCount>> GetAssetsByLocationAsync();

    Task<PagedResult<AssetMovementResponse>> GetRecentMovementsAsync(AssetMovementListQuery query);
}
