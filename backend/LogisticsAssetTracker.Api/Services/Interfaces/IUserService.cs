using LogisticsAssetTracker.Api.Common;
using LogisticsAssetTracker.Api.Dtos.Users;

namespace LogisticsAssetTracker.Api.Services.Interfaces;

public interface IUserService
{
    Task<PagedResult<UserResponse>> ListAsync(UserListQuery query);
    Task<UserResponse> CreateAsync(CreateUserRequest request, Guid actingUserId);
    Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request, Guid actingUserId);
}
