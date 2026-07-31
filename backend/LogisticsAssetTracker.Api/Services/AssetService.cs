using LogisticsAssetTracker.Api.Common;
using LogisticsAssetTracker.Api.Data;
using LogisticsAssetTracker.Api.Domain.Entities;
using LogisticsAssetTracker.Api.Domain.Enums;
using LogisticsAssetTracker.Api.Dtos.Assets;
using LogisticsAssetTracker.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LogisticsAssetTracker.Api.Services;

public class AssetService : IAssetService
{
    private const int MaxAssetCodeAttempts = 20;

    private readonly AppDbContext _db;
    private readonly IAuditLogService _auditLog;

    public AssetService(AppDbContext db, IAuditLogService auditLog)
    {
        _db = db;
        _auditLog = auditLog;
    }

    public async Task<PagedResult<AssetResponse>> ListAsync(AssetListQuery query, UserRole requesterRole)
    {
        var assets = _db.Assets.Include(a => a.CurrentLocation).Include(a => a.AssignedToUser).AsQueryable();

        // Operators see active assets only (Document 06).
        if (requesterRole == UserRole.Operator)
        {
            assets = assets.Where(a => a.IsActive);
        }
        else if (query.IsActive.HasValue)
        {
            assets = assets.Where(a => a.IsActive == query.IsActive.Value);
        }

        if (query.Status.HasValue)
        {
            assets = assets.Where(a => a.Status == query.Status.Value);
        }

        if (query.Condition.HasValue)
        {
            assets = assets.Where(a => a.Condition == query.Condition.Value);
        }

        if (query.LocationId.HasValue)
        {
            assets = assets.Where(a => a.CurrentLocationId == query.LocationId.Value);
        }

        if (query.Type.HasValue)
        {
            assets = assets.Where(a => a.Type == query.Type.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLowerInvariant();
            assets = assets.Where(a =>
                a.Name.ToLower().Contains(search) ||
                a.AssetCode.ToLower().Contains(search));
        }

        assets = (query.SortBy?.ToLowerInvariant(), query.SortDirection?.ToLowerInvariant()) switch
        {
            ("name", "desc") => assets.OrderByDescending(a => a.Name),
            ("name", _) => assets.OrderBy(a => a.Name),
            ("assetcode", "desc") => assets.OrderByDescending(a => a.AssetCode),
            ("assetcode", _) => assets.OrderBy(a => a.AssetCode),
            ("status", "desc") => assets.OrderByDescending(a => a.Status),
            ("status", _) => assets.OrderBy(a => a.Status),
            (_, "asc") => assets.OrderBy(a => a.UpdatedAt),
            _ => assets.OrderByDescending(a => a.UpdatedAt)
        };

        var totalCount = await assets.CountAsync();
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

        var items = await assets
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AssetResponse
            {
                Id = a.Id,
                AssetCode = a.AssetCode,
                Name = a.Name,
                Type = a.Type,
                CurrentLocationId = a.CurrentLocationId,
                CurrentLocationName = a.CurrentLocation != null ? a.CurrentLocation.Name : string.Empty,
                Status = a.Status,
                Condition = a.Condition,
                AssignedToUserId = a.AssignedToUserId,
                AssignedToUserName = a.AssignedToUser != null ? a.AssignedToUser.FullName : null,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            })
            .ToListAsync();

        return new PagedResult<AssetResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<AssetResponse> GetByIdAsync(Guid id, UserRole requesterRole)
    {
        var asset = await _db.Assets
            .Include(a => a.CurrentLocation)
            .Include(a => a.AssignedToUser)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (asset is null || (requesterRole == UserRole.Operator && !asset.IsActive))
        {
            throw ApiException.NotFound("ASSET_NOT_FOUND", "Asset not found.");
        }

        return MapToResponse(asset);
    }

    public async Task<AssetResponse> CreateAsync(CreateAssetRequest request, Guid actingUserId)
    {
        // Document 06 asset-creation rules.
        if (request.Status == AssetStatus.Retired)
        {
            throw ApiException.Unprocessable("RETIRED_NOT_ALLOWED_ON_CREATE", "Retired assets must not be created directly.");
        }

        if (request.Status == AssetStatus.Available && request.Condition != AssetCondition.Good)
        {
            throw ApiException.Unprocessable("INVALID_ASSET_STATE", "If initial status is Available, initial condition must be Good.");
        }

        var location = await _db.Locations.FindAsync(request.CurrentLocationId)
            ?? throw ApiException.NotFound("LOCATION_NOT_FOUND", "Location not found.");

        if (request.AssignedToUserId.HasValue)
        {
            var assignedUserExists = await _db.Users.AnyAsync(u => u.Id == request.AssignedToUserId.Value);
            if (!assignedUserExists)
            {
                throw ApiException.NotFound("USER_NOT_FOUND", "Assigned user not found.");
            }
        }

        var assetCode = await GenerateAssetCodeAsync();

        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            AssetCode = assetCode,
            NormalizedAssetCode = assetCode.ToUpperInvariant(),
            Name = request.Name.Trim(),
            Type = request.Type,
            CurrentLocationId = request.CurrentLocationId,
            Status = request.Status,
            Condition = request.Condition,
            AssignedToUserId = request.AssignedToUserId,
            QrCodeValue = Guid.NewGuid().ToString("N"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Assets.Add(asset);
        // Asset creation is not a movement update (Document 05/06): no AssetMovement is created here.
        _auditLog.Log(actingUserId, AuditActions.AssetCreated, AuditEntityTypes.Asset, asset.Id,
            new { asset.AssetCode, asset.Name, asset.Type, asset.Status, asset.Condition, asset.CurrentLocationId });

        await _db.SaveChangesAsync();

        asset.CurrentLocation = location;
        return MapToResponse(asset);
    }

    public async Task<AssetResponse> UpdateAsync(Guid id, UpdateAssetRequest request, Guid actingUserId)
    {
        var asset = await _db.Assets
            .Include(a => a.CurrentLocation)
            .Include(a => a.AssignedToUser)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw ApiException.NotFound("ASSET_NOT_FOUND", "Asset not found.");

        if (request.AssignedToUserId.HasValue && request.AssignedToUserId != asset.AssignedToUserId)
        {
            var assignedUserExists = await _db.Users.AnyAsync(u => u.Id == request.AssignedToUserId.Value);
            if (!assignedUserExists)
            {
                throw ApiException.NotFound("USER_NOT_FOUND", "Assigned user not found.");
            }
        }

        // Edit boundary (Document 04/06): only name, type, and assignedToUserId may change here.
        // currentLocationId, status, condition, assetCode, and qrCodeValue are untouched.
        asset.Name = request.Name.Trim();
        asset.Type = request.Type;
        asset.AssignedToUserId = request.AssignedToUserId;
        asset.UpdatedAt = DateTime.UtcNow;

        _auditLog.Log(actingUserId, AuditActions.AssetUpdated, AuditEntityTypes.Asset, asset.Id,
            new { asset.Name, asset.Type, asset.AssignedToUserId });

        await _db.SaveChangesAsync();

        if (asset.AssignedToUserId.HasValue && asset.AssignedToUser?.Id != asset.AssignedToUserId)
        {
            asset.AssignedToUser = await _db.Users.FindAsync(asset.AssignedToUserId.Value);
        }
        else if (!asset.AssignedToUserId.HasValue)
        {
            asset.AssignedToUser = null;
        }

        return MapToResponse(asset);
    }

    public async Task DeactivateAsync(Guid id, Guid actingUserId)
    {
        var asset = await _db.Assets.FindAsync(id)
            ?? throw ApiException.NotFound("ASSET_NOT_FOUND", "Asset not found.");

        if (!asset.IsActive)
        {
            return;
        }

        asset.IsActive = false;
        asset.UpdatedAt = DateTime.UtcNow;

        _auditLog.Log(actingUserId, AuditActions.AssetDeactivated, AuditEntityTypes.Asset, asset.Id);

        await _db.SaveChangesAsync();
    }

    public async Task<AssetQrResponse> GetQrAsync(Guid id)
    {
        var asset = await _db.Assets.FindAsync(id)
            ?? throw ApiException.NotFound("ASSET_NOT_FOUND", "Asset not found.");

        return new AssetQrResponse
        {
            AssetId = asset.Id,
            AssetCode = asset.AssetCode,
            QrCodeValue = asset.QrCodeValue,
            QrUrl = $"/assets/qr/{asset.QrCodeValue}/update"
        };
    }

    private async Task<string> GenerateAssetCodeAsync()
    {
        var startingSequence = await _db.Assets.CountAsync() + 1;

        for (var attempt = 0; attempt < MaxAssetCodeAttempts; attempt++)
        {
            var candidate = $"ASSET-{startingSequence + attempt:D6}";
            var exists = await _db.Assets.AnyAsync(a => a.NormalizedAssetCode == candidate);
            if (!exists)
            {
                return candidate;
            }
        }

        throw new InvalidOperationException("Unable to generate a unique asset code after multiple attempts.");
    }

    private static AssetResponse MapToResponse(Asset asset) => new()
    {
        Id = asset.Id,
        AssetCode = asset.AssetCode,
        Name = asset.Name,
        Type = asset.Type,
        CurrentLocationId = asset.CurrentLocationId,
        CurrentLocationName = asset.CurrentLocation?.Name ?? string.Empty,
        Status = asset.Status,
        Condition = asset.Condition,
        AssignedToUserId = asset.AssignedToUserId,
        AssignedToUserName = asset.AssignedToUser?.FullName,
        IsActive = asset.IsActive,
        CreatedAt = asset.CreatedAt,
        UpdatedAt = asset.UpdatedAt
    };
}
