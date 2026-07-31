using LogisticsAssetTracker.Api.Common;
using LogisticsAssetTracker.Api.Domain.Enums;
using LogisticsAssetTracker.Api.Dtos.Locations;

namespace LogisticsAssetTracker.Api.Services.Interfaces;

public interface ILocationService
{
    Task<PagedResult<LocationResponse>> ListAsync(LocationListQuery query, UserRole requesterRole);
    Task<LocationResponse> CreateAsync(CreateLocationRequest request, Guid actingUserId);
    Task<LocationResponse> UpdateAsync(Guid id, UpdateLocationRequest request, Guid actingUserId);
    Task DeactivateAsync(Guid id, Guid actingUserId);
}
