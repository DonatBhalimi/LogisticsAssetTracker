using LogisticsAssetTracker.Api.Common;
using LogisticsAssetTracker.Api.Data;
using LogisticsAssetTracker.Api.Domain.Entities;
using LogisticsAssetTracker.Api.Domain.Enums;
using LogisticsAssetTracker.Api.Dtos.Movements;
using LogisticsAssetTracker.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LogisticsAssetTracker.Api.Services;

// Phase 3 scope (Document 10): only location-only movement updates are supported.
// Status and condition must remain unchanged; any requested change is rejected as
// unsupported until Phase 4 implements full Document 05 status/condition rules.
public class AssetMovementService : IAssetMovementService
{
    private readonly AppDbContext _db;
    private readonly IAuditLogService _auditLog;

    public AssetMovementService(AppDbContext db, IAuditLogService auditLog)
    {
        _db = db;
        _auditLog = auditLog;
    }

    public async Task<PagedResult<AssetMovementResponse>> GetHistoryAsync(Guid assetId, AssetMovementListQuery query, UserRole requesterRole)
    {
        var asset = await _db.Assets.FindAsync(assetId)
            ?? throw ApiException.NotFound("ASSET_NOT_FOUND", "Asset not found.");

        // Operators may view movement history for active assets only (Document 06).
        if (requesterRole == UserRole.Operator && !asset.IsActive)
        {
            throw ApiException.NotFound("ASSET_NOT_FOUND", "Asset not found.");
        }

        var movements = _db.AssetMovements
            .Include(m => m.FromLocation)
            .Include(m => m.ToLocation)
            .Include(m => m.UpdatedByUser)
            .Where(m => m.AssetId == assetId)
            .AsQueryable();

        if (query.SourceType.HasValue)
        {
            movements = movements.Where(m => m.SourceType == query.SourceType.Value);
        }

        // Operators must not use isSuspicious as a filter (Document 06).
        if (query.IsSuspicious.HasValue && requesterRole != UserRole.Operator)
        {
            movements = movements.Where(m => m.IsSuspicious == query.IsSuspicious.Value);
        }

        if (query.FromDate.HasValue)
        {
            movements = movements.Where(m => m.CreatedAt >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            movements = movements.Where(m => m.CreatedAt <= query.ToDate.Value);
        }

        if (query.UpdatedByUserId.HasValue)
        {
            movements = movements.Where(m => m.UpdatedByUserId == query.UpdatedByUserId.Value);
        }

        movements = query.SortDirection?.ToLowerInvariant() == "asc"
            ? movements.OrderBy(m => m.CreatedAt)
            : movements.OrderByDescending(m => m.CreatedAt);

        var totalCount = await movements.CountAsync();
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

        var items = await movements
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<AssetMovementResponse>
        {
            Items = items.Select(m => MapMovementToResponse(m, requesterRole)).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<MovementResult> CreateManualMovementAsync(Guid assetId, MovementRequest request, Guid actingUserId, UserRole actingUserRole)
    {
        var asset = await _db.Assets
            .Include(a => a.CurrentLocation)
            .Include(a => a.AssignedToUser)
            .FirstOrDefaultAsync(a => a.Id == assetId)
            ?? throw ApiException.NotFound("ASSET_NOT_FOUND", "Asset not found.");

        return await ProcessMovementAsync(asset, request, MovementSourceType.Manual, actingUserId, actingUserRole);
    }

    public async Task<MovementResult> CreateQrMovementAsync(string qrCodeValue, MovementRequest request, Guid actingUserId, UserRole actingUserRole)
    {
        var asset = await _db.Assets
            .Include(a => a.CurrentLocation)
            .Include(a => a.AssignedToUser)
            .FirstOrDefaultAsync(a => a.QrCodeValue == qrCodeValue)
            ?? throw ApiException.NotFound("ASSET_NOT_FOUND", "Asset not found.");

        return await ProcessMovementAsync(asset, request, MovementSourceType.QR, actingUserId, actingUserRole);
    }

    // Document 05 validation order, restricted to Phase 3 (location-only) scope.
    private async Task<MovementResult> ProcessMovementAsync(
        Asset asset, MovementRequest request, MovementSourceType sourceType, Guid actingUserId, UserRole actingUserRole)
    {
        if (!asset.IsActive)
        {
            await RecordBlockedAttemptAsync(asset, sourceType, actingUserId, "ASSET_INACTIVE", "Inactive assets cannot receive movement updates.");
            throw ApiException.Conflict("ASSET_INACTIVE", "Inactive assets cannot receive movement updates.");
        }

        if (asset.Status == AssetStatus.Retired)
        {
            await RecordBlockedAttemptAsync(asset, sourceType, actingUserId, "ASSET_RETIRED", "Retired assets cannot receive normal movement updates.");
            throw ApiException.Unprocessable("ASSET_RETIRED", "Retired assets cannot receive normal movement updates.");
        }

        var locationChanging = request.ToLocationId.HasValue && request.ToLocationId.Value != asset.CurrentLocationId;
        Location? destination = null;

        if (request.ToLocationId.HasValue)
        {
            destination = await _db.Locations.FindAsync(request.ToLocationId.Value)
                ?? throw ApiException.NotFound("LOCATION_NOT_FOUND", "Location not found.");

            if (locationChanging && !destination.IsActive)
            {
                await RecordBlockedAttemptAsync(asset, sourceType, actingUserId, "LOCATION_INACTIVE", "Inactive locations cannot be selected as new movement destinations.");
                throw ApiException.Conflict("LOCATION_INACTIVE", "Inactive locations cannot be selected as new movement destinations.");
            }
        }

        var statusChanging = request.NewStatus.HasValue && request.NewStatus.Value != asset.Status;
        var conditionChanging = request.NewCondition.HasValue && request.NewCondition.Value != asset.Condition;

        if (!locationChanging && !statusChanging && !conditionChanging)
        {
            await RecordBlockedAttemptAsync(asset, sourceType, actingUserId, "NO_STATE_CHANGE", "At least one of location, status, or condition must change.");
            throw ApiException.Unprocessable("NO_STATE_CHANGE", "At least one of location, status, or condition must change.");
        }

        // Phase 3 restriction (Document 10): status/condition changes are Phase 4 scope.
        if (statusChanging)
        {
            await RecordBlockedAttemptAsync(asset, sourceType, actingUserId, "STATUS_CHANGE_NOT_SUPPORTED", "Status changes are not supported until Phase 4.");
            throw ApiException.Unprocessable("INVALID_MOVEMENT", "Status changes are not supported until Phase 4.");
        }

        if (conditionChanging)
        {
            await RecordBlockedAttemptAsync(asset, sourceType, actingUserId, "CONDITION_CHANGE_NOT_SUPPORTED", "Condition changes are not supported until Phase 4.");
            throw ApiException.Unprocessable("INVALID_MOVEMENT", "Condition changes are not supported until Phase 4.");
        }

        // Only a real location change reaches this point in Phase 3.
        var previousLocationId = asset.CurrentLocationId;
        var previousLocation = asset.CurrentLocation;

        var movement = new AssetMovement
        {
            Id = Guid.NewGuid(),
            AssetId = asset.Id,
            FromLocationId = previousLocationId,
            ToLocationId = request.ToLocationId!.Value,
            PreviousStatus = asset.Status,
            NewStatus = asset.Status,
            PreviousCondition = asset.Condition,
            NewCondition = asset.Condition,
            SourceType = sourceType,
            UpdatedByUserId = actingUserId,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            IsSuspicious = false,
            SuspiciousReason = null,
            CreatedAt = DateTime.UtcNow
        };

        _db.AssetMovements.Add(movement);

        asset.CurrentLocationId = movement.ToLocationId;
        asset.CurrentLocation = destination;
        asset.UpdatedAt = DateTime.UtcNow;

        _auditLog.Log(actingUserId, AuditActions.MovementCreated, AuditEntityTypes.AssetMovement, movement.Id,
            new { AssetId = asset.Id, FromLocationId = previousLocationId, movement.ToLocationId, SourceType = sourceType.ToString() });

        await _db.SaveChangesAsync();

        movement.FromLocation = previousLocation;
        movement.ToLocation = destination;
        movement.UpdatedByUser = await _db.Users.FindAsync(actingUserId);

        return new MovementResult
        {
            ResultType = "MovementCreated",
            Movement = MapMovementToResponse(movement, actingUserRole),
            Asset = AssetService.MapToResponse(asset)
        };
    }

    private async Task RecordBlockedAttemptAsync(Asset asset, MovementSourceType sourceType, Guid actingUserId, string reasonCode, string reasonMessage)
    {
        _auditLog.Log(actingUserId, AuditActions.MovementBlocked, AuditEntityTypes.Asset, asset.Id,
            new { AssetId = asset.Id, SourceType = sourceType.ToString(), ReasonCode = reasonCode, ReasonMessage = reasonMessage });

        await _db.SaveChangesAsync();
    }

    private static AssetMovementResponse MapMovementToResponse(AssetMovement movement, UserRole requesterRole)
    {
        var response = new AssetMovementResponse
        {
            Id = movement.Id,
            AssetId = movement.AssetId,
            FromLocationId = movement.FromLocationId,
            FromLocationName = movement.FromLocation?.Name ?? string.Empty,
            ToLocationId = movement.ToLocationId,
            ToLocationName = movement.ToLocation?.Name ?? string.Empty,
            PreviousStatus = movement.PreviousStatus,
            NewStatus = movement.NewStatus,
            PreviousCondition = movement.PreviousCondition,
            NewCondition = movement.NewCondition,
            SourceType = movement.SourceType,
            UpdatedByUserId = movement.UpdatedByUserId,
            UpdatedByUserName = movement.UpdatedByUser?.FullName ?? string.Empty,
            Notes = movement.Notes,
            CreatedAt = movement.CreatedAt
        };

        // Document 06: Operators must not see suspicious indicators or reasons.
        if (requesterRole != UserRole.Operator)
        {
            response.IsSuspicious = movement.IsSuspicious;
            response.SuspiciousReason = movement.SuspiciousReason;
        }

        return response;
    }
}
