using LogisticsAssetTracker.Api.Dtos.Auth;

namespace LogisticsAssetTracker.Api.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<UserSummaryResponse> GetCurrentUserAsync(Guid userId);
}
