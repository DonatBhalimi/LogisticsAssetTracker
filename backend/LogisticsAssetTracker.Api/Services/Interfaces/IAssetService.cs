using LogisticsAssetTracker.Api.Common;
using LogisticsAssetTracker.Api.Domain.Enums;
using LogisticsAssetTracker.Api.Dtos.Assets;

namespace LogisticsAssetTracker.Api.Services.Interfaces;

public interface IAssetService
{
    Task<PagedResult<AssetResponse>> ListAsync(AssetListQuery query, UserRole requesterRole);
    Task<AssetResponse> GetByIdAsync(Guid id, UserRole requesterRole);
    Task<AssetResponse> CreateAsync(CreateAssetRequest request, Guid actingUserId);
    Task<AssetResponse> UpdateAsync(Guid id, UpdateAssetRequest request, Guid actingUserId);
    Task DeactivateAsync(Guid id, Guid actingUserId);
    Task<AssetQrResponse> GetQrAsync(Guid id);
}
