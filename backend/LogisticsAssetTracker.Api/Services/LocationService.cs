using LogisticsAssetTracker.Api.Common;
using LogisticsAssetTracker.Api.Data;
using LogisticsAssetTracker.Api.Domain.Entities;
using LogisticsAssetTracker.Api.Domain.Enums;
using LogisticsAssetTracker.Api.Dtos.Locations;
using LogisticsAssetTracker.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LogisticsAssetTracker.Api.Services;

public class LocationService : ILocationService
{
    private readonly AppDbContext _db;
    private readonly IAuditLogService _auditLog;

    public LocationService(AppDbContext db, IAuditLogService auditLog)
    {
        _db = db;
        _auditLog = auditLog;
    }

    public async Task<PagedResult<LocationResponse>> ListAsync(LocationListQuery query, UserRole requesterRole)
    {
        var locations = _db.Locations.AsQueryable();

        // Operators see active locations only (Document 06).
        if (requesterRole == UserRole.Operator)
        {
            locations = locations.Where(l => l.IsActive);
        }
        else if (query.IsActive.HasValue)
        {
            locations = locations.Where(l => l.IsActive == query.IsActive.Value);
        }

        if (query.Type.HasValue)
        {
            locations = locations.Where(l => l.Type == query.Type.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLowerInvariant();
            locations = locations.Where(l => l.Name.ToLower().Contains(search));
        }

        locations = (query.SortBy?.ToLowerInvariant(), query.SortDirection?.ToLowerInvariant()) switch
        {
            ("name", "desc") => locations.OrderByDescending(l => l.Name),
            ("name", _) => locations.OrderBy(l => l.Name),
            ("type", "desc") => locations.OrderByDescending(l => l.Type),
            ("type", _) => locations.OrderBy(l => l.Type),
            (_, "asc") => locations.OrderBy(l => l.CreatedAt),
            _ => locations.OrderByDescending(l => l.CreatedAt)
        };

        var totalCount = await locations.CountAsync();
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

        var items = await locations
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LocationResponse
            {
                Id = l.Id,
                Name = l.Name,
                Type = l.Type,
                Address = l.Address,
                Description = l.Description,
                IsActive = l.IsActive,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            })
            .ToListAsync();

        return new PagedResult<LocationResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<LocationResponse> CreateAsync(CreateLocationRequest request, Guid actingUserId)
    {
        var location = new Location
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Type = request.Type,
            Address = request.Address?.Trim(),
            Description = request.Description?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Locations.Add(location);
        _auditLog.Log(actingUserId, AuditActions.LocationCreated, AuditEntityTypes.Location, location.Id,
            new { location.Name, location.Type });

        await _db.SaveChangesAsync();

        return MapToResponse(location);
    }

    public async Task<LocationResponse> UpdateAsync(Guid id, UpdateLocationRequest request, Guid actingUserId)
    {
        var location = await _db.Locations.FindAsync(id)
            ?? throw ApiException.NotFound("LOCATION_NOT_FOUND", "Location not found.");

        location.Name = request.Name.Trim();
        location.Type = request.Type;
        location.Address = request.Address?.Trim();
        location.Description = request.Description?.Trim();
        location.UpdatedAt = DateTime.UtcNow;

        _auditLog.Log(actingUserId, AuditActions.LocationUpdated, AuditEntityTypes.Location, location.Id,
            new { location.Name, location.Type });

        await _db.SaveChangesAsync();

        return MapToResponse(location);
    }

    public async Task DeactivateAsync(Guid id, Guid actingUserId)
    {
        var location = await _db.Locations.FindAsync(id)
            ?? throw ApiException.NotFound("LOCATION_NOT_FOUND", "Location not found.");

        if (!location.IsActive)
        {
            return;
        }

        var hasActiveAssets = await _db.Assets.AnyAsync(a => a.CurrentLocationId == id && a.IsActive);
        if (hasActiveAssets)
        {
            throw ApiException.Conflict("LOCATION_IN_USE", "This location cannot be deactivated because active assets currently reference it.");
        }

        location.IsActive = false;
        location.UpdatedAt = DateTime.UtcNow;

        _auditLog.Log(actingUserId, AuditActions.LocationDeactivated, AuditEntityTypes.Location, location.Id);

        await _db.SaveChangesAsync();
    }

    private static LocationResponse MapToResponse(Location location) => new()
    {
        Id = location.Id,
        Name = location.Name,
        Type = location.Type,
        Address = location.Address,
        Description = location.Description,
        IsActive = location.IsActive,
        CreatedAt = location.CreatedAt,
        UpdatedAt = location.UpdatedAt
    };
}
