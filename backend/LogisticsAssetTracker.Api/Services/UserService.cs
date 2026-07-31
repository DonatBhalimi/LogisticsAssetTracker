using LogisticsAssetTracker.Api.Common;
using LogisticsAssetTracker.Api.Data;
using LogisticsAssetTracker.Api.Domain.Entities;
using LogisticsAssetTracker.Api.Dtos.Users;
using LogisticsAssetTracker.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LogisticsAssetTracker.Api.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;
    private readonly IAuditLogService _auditLog;

    public UserService(AppDbContext db, IAuditLogService auditLog)
    {
        _db = db;
        _auditLog = auditLog;
    }

    public async Task<PagedResult<UserResponse>> ListAsync(UserListQuery query)
    {
        var users = _db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLowerInvariant();
            users = users.Where(u =>
                u.FullName.ToLower().Contains(search) ||
                u.Email.ToLower().Contains(search));
        }

        if (query.Role.HasValue)
        {
            users = users.Where(u => u.Role == query.Role.Value);
        }

        if (query.IsActive.HasValue)
        {
            users = users.Where(u => u.IsActive == query.IsActive.Value);
        }

        users = (query.SortBy?.ToLowerInvariant(), query.SortDirection?.ToLowerInvariant()) switch
        {
            ("fullname", "desc") => users.OrderByDescending(u => u.FullName),
            ("fullname", _) => users.OrderBy(u => u.FullName),
            ("email", "desc") => users.OrderByDescending(u => u.Email),
            ("email", _) => users.OrderBy(u => u.Email),
            ("role", "desc") => users.OrderByDescending(u => u.Role),
            ("role", _) => users.OrderBy(u => u.Role),
            (_, "asc") => users.OrderBy(u => u.CreatedAt),
            _ => users.OrderByDescending(u => u.CreatedAt)
        };

        var totalCount = await users.CountAsync();
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

        var items = await users
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            })
            .ToListAsync();

        return new PagedResult<UserResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, Guid actingUserId)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var emailExists = await _db.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail);
        if (emailExists)
        {
            throw ApiException.Conflict("EMAIL_ALREADY_EXISTS", "A user with this email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            IsActive = request.IsActive ?? true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        _auditLog.Log(actingUserId, AuditActions.UserCreated, AuditEntityTypes.User, user.Id,
            new { user.FullName, user.Email, user.Role, user.IsActive });

        await _db.SaveChangesAsync();

        return MapToResponse(user);
    }

    public async Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request, Guid actingUserId)
    {
        var user = await _db.Users.FindAsync(id)
            ?? throw ApiException.NotFound("USER_NOT_FOUND", "User not found.");

        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var emailInUse = await _db.Users.AnyAsync(u => u.Id != id && u.NormalizedEmail == normalizedEmail);
        if (emailInUse)
        {
            throw ApiException.Conflict("EMAIL_ALREADY_EXISTS", "A user with this email already exists.");
        }

        user.FullName = request.FullName.Trim();
        user.Email = request.Email.Trim();
        user.NormalizedEmail = normalizedEmail;
        user.Role = request.Role;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        _auditLog.Log(actingUserId, AuditActions.UserUpdated, AuditEntityTypes.User, user.Id,
            new { user.FullName, user.Email, user.Role, user.IsActive });

        await _db.SaveChangesAsync();

        return MapToResponse(user);
    }

    private static UserResponse MapToResponse(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        Role = user.Role,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };
}
