using LogisticsAssetTracker.Api.Common;
using LogisticsAssetTracker.Api.Domain.Enums;
using LogisticsAssetTracker.Api.Dtos.Assets;
using LogisticsAssetTracker.Api.Dtos.Movements;

namespace LogisticsAssetTracker.Api.Services.Interfaces;

public interface IAssetMovementService
{
    Task<PagedResult<AssetMovementResponse>> GetHistoryAsync(Guid assetId, AssetMovementListQuery query, UserRole requesterRole);

    Task<MovementResult> CreateManualMovementAsync(Guid assetId, MovementRequest request, Guid actingUserId, UserRole actingUserRole);

    Task<MovementResult> CreateQrMovementAsync(string qrCodeValue, MovementRequest request, Guid actingUserId, UserRole actingUserRole);

    Task<MovementResult> ReactivateAsync(Guid assetId, ReactivateAssetRequest request, Guid actingUserId, UserRole actingUserRole);
}
