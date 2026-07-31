using LogisticsAssetTracker.Api.Auth;
using LogisticsAssetTracker.Api.Common;
using LogisticsAssetTracker.Api.Data;
using LogisticsAssetTracker.Api.Domain.Entities;
using LogisticsAssetTracker.Api.Dtos.Auth;
using LogisticsAssetTracker.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LogisticsAssetTracker.Api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(AppDbContext db, IJwtTokenService jwtTokenService)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw ApiException.Unauthorized("INVALID_CREDENTIALS", "Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw ApiException.Unauthorized("ACCOUNT_INACTIVE", "This account is inactive.");
        }

        var token = _jwtTokenService.GenerateToken(user);

        return new LoginResponse
        {
            Token = token,
            User = MapToSummary(user)
        };
    }

    public async Task<UserSummaryResponse> GetCurrentUserAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw ApiException.Unauthorized("INVALID_CREDENTIALS", "User not found.");

        return MapToSummary(user);
    }

    private static UserSummaryResponse MapToSummary(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        Role = user.Role,
        IsActive = user.IsActive
    };
}
