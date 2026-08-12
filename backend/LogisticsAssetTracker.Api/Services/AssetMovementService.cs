using System.Text.Json;
using LogisticsAssetTracker.Api.Common;
using LogisticsAssetTracker.Api.Data;
using LogisticsAssetTracker.Api.Domain.Entities;
using LogisticsAssetTracker.Api.Domain.Enums;
using LogisticsAssetTracker.Api.Dtos.Approvals;
using LogisticsAssetTracker.Api.Dtos.Assets;
using LogisticsAssetTracker.Api.Dtos.Movements;
using LogisticsAssetTracker.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LogisticsAssetTracker.Api.Services;

// Phase 4 (Document 05): full status/condition/Lost/Retired movement rules, inspection
// clearance, Operator Lost-to-Available approval requests, Manager/Admin direct Lost
// reactivation, Admin Lost-to-Retired, and suspicious-activity detection.
public class AssetMovementService : IAssetMovementService
{
    private readonly AppDbContext _db;
    private readonly IAuditLogService _auditLog;
    private readonly MovementThresholdsOptions _thresholds;

    public AssetMovementService(AppDbContext db, IAuditLogService auditLog, IOptions<MovementThresholdsOptions> thresholds)
    {
        _db = db;
        _auditLog = auditLog;
        _thresholds = thresholds.Value;
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

    // Manager/Admin direct Lost reactivation (Document 06 POST /api/assets/{id}/reactivate).
    // Dedicated flow: does not use the approval queue and is rejected while one is pending.
    public async Task<MovementResult> ReactivateAsync(Guid assetId, ReactivateAssetRequest request, Guid actingUserId, UserRole actingUserRole)
    {
        var asset = await _db.Assets
            .Include(a => a.CurrentLocation)
            .Include(a => a.AssignedToUser)
            .FirstOrDefaultAsync(a => a.Id == assetId)
            ?? throw ApiException.NotFound("ASSET_NOT_FOUND", "Asset not found.");

        var notes = request.DecisionNote.Trim();

        if (!asset.IsActive)
        {
            await RecordBlockedAttemptAsync(asset, MovementSourceType.Manual, actingUserId, "ASSET_INACTIVE", "Inactive assets cannot receive movement updates.", request.ToLocationId);
            throw ApiException.Conflict("ASSET_INACTIVE", "Inactive assets cannot receive movement updates.");
        }

        if (asset.Status != AssetStatus.Lost)
        {
            await RecordBlockedAttemptAsync(asset, MovementSourceType.Manual, actingUserId, "ASSET_NOT_LOST", "Direct reactivation requires the asset to be currently Lost.", request.ToLocationId);
            throw ApiException.Unprocessable("ASSET_NOT_LOST", "Direct reactivation requires the asset to be currently Lost.");
        }

        if (asset.Condition != AssetCondition.Good)
        {
            await RecordBlockedAttemptAsync(asset, MovementSourceType.Manual, actingUserId, "LOST_CONDITION_NOT_GOOD", "Manager/Admin must clear the condition to Good before reactivation.", request.ToLocationId);
            throw ApiException.Unprocessable("LOST_CONDITION_NOT_GOOD", "Manager/Admin must clear the condition to Good before reactivation.");
        }

        var hasPendingApproval = await _db.MovementApprovals.AnyAsync(a => a.AssetId == asset.Id && a.ApprovalStatus == ApprovalStatus.Pending);
        if (hasPendingApproval)
        {
            await RecordBlockedAttemptAsync(asset, MovementSourceType.Manual, actingUserId, "PENDING_APPROVAL_EXISTS", "A pending Lost-reactivation approval already exists for this asset.", request.ToLocationId);
            throw ApiException.Conflict("PENDING_APPROVAL_EXISTS", "A pending Lost-reactivation approval already exists for this asset.");
        }

        Location? destination = asset.CurrentLocation;
        var toLocationId = asset.CurrentLocationId;
        if (request.ToLocationId.HasValue && request.ToLocationId.Value != asset.CurrentLocationId)
        {
            destination = await _db.Locations.FindAsync(request.ToLocationId.Value)
                ?? throw ApiException.NotFound("LOCATION_NOT_FOUND", "Location not found.");

            if (!destination.IsActive)
            {
                await RecordBlockedAttemptAsync(asset, MovementSourceType.Manual, actingUserId, "LOCATION_INACTIVE", "Inactive locations cannot be selected as new movement destinations.", request.ToLocationId);
                throw ApiException.Conflict("LOCATION_INACTIVE", "Inactive locations cannot be selected as new movement destinations.");
            }

            toLocationId = destination.Id;
        }

        var previousLocationId = asset.CurrentLocationId;
        var previousLocation = asset.CurrentLocation;
        var previousStatus = asset.Status;
        var previousCondition = asset.Condition;

        var movement = new AssetMovement
        {
            Id = Guid.NewGuid(),
            AssetId = asset.Id,
            FromLocationId = previousLocationId,
            ToLocationId = toLocationId,
            PreviousStatus = previousStatus,
            NewStatus = AssetStatus.Available,
            PreviousCondition = previousCondition,
            NewCondition = previousCondition,
            SourceType = MovementSourceType.Manual,
            UpdatedByUserId = actingUserId,
            Notes = notes,
            IsSuspicious = false,
            SuspiciousReason = null,
            CreatedAt = DateTime.UtcNow
        };

        _db.AssetMovements.Add(movement);

        asset.CurrentLocationId = toLocationId;
        asset.CurrentLocation = destination;
        asset.Status = AssetStatus.Available;
        asset.UpdatedAt = DateTime.UtcNow;

        _auditLog.Log(actingUserId, AuditActions.AssetLostReactivated, AuditEntityTypes.Asset, asset.Id,
            new { AssetId = asset.Id, PreviousStatus = previousStatus.ToString(), NewStatus = asset.Status.ToString(), DecisionNote = notes, SubmittedToLocationId = request.ToLocationId });

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

    // Document 05 backend validation order, extended to full Phase 4 scope.
    private async Task<MovementResult> ProcessMovementAsync(
        Asset asset, MovementRequest request, MovementSourceType sourceType, Guid actingUserId, UserRole actingUserRole)
    {
        if (!asset.IsActive)
        {
            await RecordBlockedAttemptAsync(asset, sourceType, actingUserId, "ASSET_INACTIVE", "Inactive assets cannot receive movement updates.", request.ToLocationId);
            throw ApiException.Conflict("ASSET_INACTIVE", "Inactive assets cannot receive movement updates.");
        }

        if (asset.Status == AssetStatus.Retired)
        {
            await RecordBlockedAttemptAsync(asset, sourceType, actingUserId, "ASSET_RETIRED", "Retired assets cannot receive normal movement updates.", request.ToLocationId);
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
                await RecordBlockedAttemptAsync(asset, sourceType, actingUserId, "LOCATION_INACTIVE", "Inactive locations cannot be selected as new movement destinations.", request.ToLocationId);
                throw ApiException.Conflict("LOCATION_INACTIVE", "Inactive locations cannot be selected as new movement destinations.");
            }
        }

        var statusChanging = request.NewStatus.HasValue && request.NewStatus.Value != asset.Status;
        var conditionChanging = request.NewCondition.HasValue && request.NewCondition.Value != asset.Condition;

        if (!locationChanging && !statusChanging && !conditionChanging)
        {
            await RecordBlockedAttemptAsync(asset, sourceType, actingUserId, "NO_STATE_CHANGE", "At least one of location, status, or condition must change.", request.ToLocationId);
            throw ApiException.Unprocessable("NO_STATE_CHANGE", "At least one of location, status, or condition must change.");
        }

        var currentStatus = asset.Status;
        var currentCondition = asset.Condition;
        var requestedStatus = request.NewStatus ?? currentStatus;
        var requestedCondition = request.NewCondition ?? currentCondition;
        var notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        var specialAuditAction = AuditActions.MovementCreated;

        if (currentStatus == AssetStatus.Lost && statusChanging)
        {
            if (requestedStatus == AssetStatus.Available)
            {
                if (actingUserRole == UserRole.Operator)
                {
                    return await CreateLostReactivationApprovalAsync(asset, request, sourceType, actingUserId, currentCondition, notes);
                }

                // Manager/Admin must use the dedicated /reactivate endpoint (Document 05/06).
                await RecordBlockedAttemptAsync(asset, sourceType, actingUserId, "INVALID_MOVEMENT",
                    "Manager/Admin Lost-to-Available must use the dedicated direct-reactivation flow.", request.ToLocationId,
                    isSuspicious: true, suspiciousReason: "Unauthorized direct Lost-to-Available attempt outside the dedicated reactivation flow.");
                throw ApiException.Unprocessable("INVALID_MOVEMENT", "Manager/Admin Lost-to-Available must use the dedicated direct-reactivation flow.");
            }

            if (requestedStatus == AssetStatus.Retired)
            {
                if (actingUserRole != UserRole.Admin)
                {
                    await RecordBlockedAttemptAsync(asset, sourceType, actingUserId, "ADMIN_ONLY", "Lost-to-Retired is Admin-only.", request.ToLocationId);
                    throw ApiException.Forbidden("ADMIN_ONLY", "Lost-to-Retired is Admin-only.");
                }

                if (sourceType != MovementSourceType.Manual)
                {
                    await RecordBlockedAttemptAsync(asset, sourceType, actingUserId, "INVALID_MOVEMENT", "Lost-to-Retired must use the manual movement flow.", request.ToLocationId);
                    throw ApiException.Unprocessable("INVALID_MOVEMENT", "Lost-to-Retired must use the manual movement flow.");
                }

                if (string.IsNullOrEmpty(notes))
                {
                    await RecordBlockedAttemptAsync(asset, sourceType, actingUserId, "NOTES_REQUIRED", "Lost-to-Retired requires a non-empty retirement note.", request.ToLocationId);
                    throw ApiException.Unprocessable("NOTES_REQUIRED", "Lost-to-Retired requires a non-empty retirement note.");
                }

                specialAuditAction = AuditActions.AssetLostToRetired;
            }
            else
            {
                // Any other transition out of Lost is prohibited (Document 05).
                await RecordBlockedAttemptAsync(asset, sourceType, actingUserId, "INVALID_MOVEMENT",
                    $"Lost cannot transition to {requestedStatus} through normal movement update.", request.ToLocationId,
                    isSuspicious: true, suspiciousReason: $"Unauthorized direct Lost -> {requestedStatus} attempt.");
                throw ApiException.Unprocessable("INVALID_MOVEMENT", $"Lost cannot transition to {requestedStatus} through normal movement update.");
            }
        }
        else
        {
            // Available cannot move directly to Delivered.
            if (statusChanging && currentStatus == AssetStatus.Available && requestedStatus == AssetStatus.Delivered)
            {
                await RecordBlockedAttemptAsync(asset, sourceType, actingUserId, "INVALID_MOVEMENT", "Available cannot move directly to Delivered.", request.ToLocationId);
                throw ApiException.Unprocessable("INVALID_MOVEMENT", "Available cannot move directly to Delivered.");
            }

            // Retired is only reachable through the documented Lost-to-Retired flow.
            if (statusChanging && requestedStatus == AssetStatus.Retired)
            {
                await RecordBlockedAttemptAsync(asset, sourceType, actingUserId, "INVALID_MOVEMENT", "Retired can only be reached from Lost through the Lost-to-Retired flow.", request.ToLocationId);
                throw ApiException.Unprocessable("INVALID_MOVEMENT", "Retired can only be reached from Lost through the Lost-to-Retired flow.");
            }

            // A movement that changes status to Available requires the resulting condition to be Good.
            if (statusChanging && requestedStatus == AssetStatus.Available && requestedCondition != AssetCondition.Good)
            {
                await RecordBlockedAttemptAsync(asset, sourceType, actingUserId, "INVALID_MOVEMENT",
                    "A movement that changes status to Available requires the resulting condition to be Good.", request.ToLocationId,
                    isSuspicious: true, suspiciousReason: "Attempted status transition to Available while the resulting condition is Damaged or NeedsInspection.");
                throw ApiException.Unprocessable("INVALID_MOVEMENT", "A movement that changes status to Available requires the resulting condition to be Good.");
            }

            // Only Manager/Admin can clear condition back to Good (inspection clearance).
            var clearingCondition = conditionChanging && requestedCondition == AssetCondition.Good && currentCondition != AssetCondition.Good;
            if (clearingCondition)
            {
                if (actingUserRole == UserRole.Operator)
                {
                    await RecordBlockedAttemptAsync(asset, sourceType, actingUserId, "CLEARANCE_FORBIDDEN", "Operators cannot clear condition back to Good.", request.ToLocationId);
                    throw ApiException.Forbidden("CLEARANCE_FORBIDDEN", "Operators cannot clear condition back to Good.");
                }

                if (sourceType != MovementSourceType.Manual)
                {
                    await RecordBlockedAttemptAsync(asset, sourceType, actingUserId, "INVALID_MOVEMENT", "Inspection clearance must use the manual movement flow.", request.ToLocationId);
                    throw ApiException.Unprocessable("INVALID_MOVEMENT", "Inspection clearance must use the manual movement flow.");
                }

                if (string.IsNullOrEmpty(notes))
                {
                    await RecordBlockedAttemptAsync(asset, sourceType, actingUserId, "NOTES_REQUIRED", "Inspection clearance requires a non-empty note.", request.ToLocationId);
                    throw ApiException.Unprocessable("NOTES_REQUIRED", "Inspection clearance requires a non-empty note.");
                }

                specialAuditAction = AuditActions.AssetConditionCleared;
            }

            // Assets already Damaged/NeedsInspection before the request cannot transition to InTransit or Delivered.
            if (statusChanging &&
                (requestedStatus == AssetStatus.InTransit || requestedStatus == AssetStatus.Delivered) &&
                (currentCondition == AssetCondition.Damaged || currentCondition == AssetCondition.NeedsInspection))
            {
                await RecordBlockedAttemptAsync(asset, sourceType, actingUserId, "INVALID_MOVEMENT",
                    "Damaged or NeedsInspection assets cannot transition to InTransit or Delivered until cleared back to Good.", request.ToLocationId);
                throw ApiException.Unprocessable("INVALID_MOVEMENT", "Damaged or NeedsInspection assets cannot transition to InTransit or Delivered until cleared back to Good.");
            }
        }

        // Suspicious detection for the completed movement (Document 05 configurable thresholds).
        var conflictingLocation = await DetectConflictingLocationAsync(asset.Id, request.ToLocationId);
        var highFrequency = await DetectHighFrequencyAsync(asset.Id);

        var completedSuspicious = conflictingLocation || highFrequency;
        string? completedSuspiciousReason = (conflictingLocation, highFrequency) switch
        {
            (true, true) => "Conflicting destination locations submitted within a short window; high frequency of movement updates detected.",
            (true, false) => "Conflicting destination locations submitted within a short window.",
            (false, true) => "High frequency of movement updates detected.",
            _ => null
        };

        var previousLocationId = asset.CurrentLocationId;
        var previousLocation = asset.CurrentLocation;
        var previousStatusValue = currentStatus;
        var previousConditionValue = currentCondition;
        var resultToLocationId = locationChanging ? destination!.Id : asset.CurrentLocationId;

        var movement = new AssetMovement
        {
            Id = Guid.NewGuid(),
            AssetId = asset.Id,
            FromLocationId = previousLocationId,
            ToLocationId = resultToLocationId,
            PreviousStatus = previousStatusValue,
            NewStatus = requestedStatus,
            PreviousCondition = previousConditionValue,
            NewCondition = requestedCondition,
            SourceType = sourceType,
            UpdatedByUserId = actingUserId,
            Notes = notes,
            IsSuspicious = completedSuspicious,
            SuspiciousReason = completedSuspiciousReason,
            CreatedAt = DateTime.UtcNow
        };

        _db.AssetMovements.Add(movement);

        asset.CurrentLocationId = resultToLocationId;
        if (locationChanging)
        {
            asset.CurrentLocation = destination;
        }
        asset.Status = requestedStatus;
        asset.Condition = requestedCondition;
        asset.UpdatedAt = DateTime.UtcNow;

        _auditLog.Log(actingUserId, specialAuditAction, AuditEntityTypes.AssetMovement, movement.Id,
            new
            {
                AssetId = asset.Id,
                FromLocationId = previousLocationId,
                ToLocationId = resultToLocationId,
                PreviousStatus = previousStatusValue.ToString(),
                NewStatus = requestedStatus.ToString(),
                PreviousCondition = previousConditionValue.ToString(),
                NewCondition = requestedCondition.ToString(),
                SourceType = sourceType.ToString(),
                SubmittedToLocationId = request.ToLocationId
            },
            completedSuspicious, completedSuspiciousReason);

        await _db.SaveChangesAsync();

        movement.FromLocation = previousLocation;
        movement.ToLocation = locationChanging ? destination : previousLocation;
        movement.UpdatedByUser = await _db.Users.FindAsync(actingUserId);

        return new MovementResult
        {
            ResultType = "MovementCreated",
            Movement = MapMovementToResponse(movement, actingUserRole),
            Asset = AssetService.MapToResponse(asset)
        };
    }

    // Operator Lost-to-Available approval request (Document 05 "Operator Lost-to-Available Approval Request").
    private async Task<MovementResult> CreateLostReactivationApprovalAsync(
        Asset asset, MovementRequest request, MovementSourceType sourceType, Guid actingUserId, AssetCondition currentCondition, string? notes)
    {
        if (currentCondition != AssetCondition.Good)
        {
            await RecordBlockedAttemptAsync(asset, sourceType, actingUserId, "LOST_CONDITION_NOT_GOOD",
                "Manager/Admin must clear the condition to Good before reactivation.", request.ToLocationId);
            throw ApiException.Unprocessable("LOST_CONDITION_NOT_GOOD", "Manager/Admin must clear the condition to Good before reactivation.");
        }

        var hasPendingApproval = await _db.MovementApprovals.AnyAsync(a => a.AssetId == asset.Id && a.ApprovalStatus == ApprovalStatus.Pending);
        if (hasPendingApproval)
        {
            // Repeated submitted attempts count toward the threshold even when rejected as duplicates.
            var isRepeated = await IsRepeatedLostReactivationAsync(asset.Id);
            _auditLog.Log(actingUserId, AuditActions.MovementApprovalDuplicateRejected, AuditEntityTypes.MovementApproval, asset.Id,
                new { AssetId = asset.Id, SourceType = sourceType.ToString(), SubmittedToLocationId = request.ToLocationId },
                isRepeated, isRepeated ? "Repeated Lost-reactivation requests exceeded the approved threshold." : null);
            await _db.SaveChangesAsync();
            throw ApiException.Conflict("PENDING_APPROVAL_EXISTS", "A pending Lost-reactivation approval already exists for this asset.");
        }

        if (string.IsNullOrEmpty(notes))
        {
            await RecordBlockedAttemptAsync(asset, sourceType, actingUserId, "NOTES_REQUIRED", "Operator Lost-to-Available requires non-empty notes.", request.ToLocationId);
            throw ApiException.Unprocessable("NOTES_REQUIRED", "Operator Lost-to-Available requires non-empty notes.");
        }

        var approval = new MovementApproval
        {
            Id = Guid.NewGuid(),
            AssetId = asset.Id,
            RequestedByUserId = actingUserId,
            RequestedLocationId = request.ToLocationId,
            RequestedStatus = AssetStatus.Available,
            RequestedCondition = AssetCondition.Good,
            RequestedSourceType = sourceType,
            ApprovalStatus = ApprovalStatus.Pending,
            RequestReason = notes,
            DecisionNote = null,
            CreatedAt = DateTime.UtcNow,
            DecidedAt = null
        };

        _db.MovementApprovals.Add(approval);

        var suspiciousRepeat = await IsRepeatedLostReactivationAsync(asset.Id);
        _auditLog.Log(actingUserId, AuditActions.MovementApprovalRequested, AuditEntityTypes.MovementApproval, approval.Id,
            new { AssetId = asset.Id, SourceType = sourceType.ToString(), SubmittedToLocationId = request.ToLocationId, RequestReason = notes },
            suspiciousRepeat, suspiciousRepeat ? "Repeated Lost-reactivation requests exceeded the approved threshold." : null);

        await _db.SaveChangesAsync();

        if (request.ToLocationId.HasValue)
        {
            approval.RequestedLocation = await _db.Locations.FindAsync(request.ToLocationId.Value);
        }
        approval.RequestedByUser = await _db.Users.FindAsync(actingUserId);

        return new MovementResult
        {
            ResultType = "ApprovalRequired",
            Approval = MovementApprovalService.MapToResponse(approval, asset),
            AssetUnchanged = true
        };
    }

    private async Task<bool> IsRepeatedLostReactivationAsync(Guid assetId)
    {
        var windowStart = DateTime.UtcNow.AddHours(-_thresholds.RepeatedLostReactivationWindowHours);

        var recentAttempts = await _db.AuditLogs
            .Where(l => l.CreatedAt >= windowStart && l.MetadataJson != null &&
                (l.Action == AuditActions.MovementApprovalRequested || l.Action == AuditActions.MovementApprovalDuplicateRejected))
            .Select(l => l.MetadataJson)
            .ToListAsync();

        var count = recentAttempts.Count(json => TryGetAssetId(json) == assetId);

        // +1 accounts for the current attempt, which has not been persisted yet.
        return count + 1 > _thresholds.RepeatedLostReactivationCount;
    }

    private async Task<bool> DetectConflictingLocationAsync(Guid assetId, Guid? submittedToLocationId)
    {
        // Omitted toLocationId is excluded from conflicting-location detection (Document 05).
        if (!submittedToLocationId.HasValue)
        {
            return false;
        }

        var windowStart = DateTime.UtcNow.AddMinutes(-_thresholds.ConflictingLocationWindowMinutes);

        var recentLogs = await _db.AuditLogs
            .Where(l => l.CreatedAt >= windowStart && l.MetadataJson != null &&
                (l.Action == AuditActions.MovementCreated || l.Action == AuditActions.MovementBlocked ||
                 l.Action == AuditActions.MovementApprovalRequested || l.Action == AuditActions.MovementApprovalDuplicateRejected ||
                 l.Action == AuditActions.AssetLostReactivated || l.Action == AuditActions.AssetLostToRetired ||
                 l.Action == AuditActions.AssetConditionCleared))
            .Select(l => l.MetadataJson)
            .ToListAsync();

        var distinctDestinations = new HashSet<Guid>();
        foreach (var json in recentLogs)
        {
            if (TryGetAssetId(json) != assetId)
            {
                continue;
            }

            var submitted = TryGetGuidProperty(json, "SubmittedToLocationId");
            if (submitted.HasValue)
            {
                distinctDestinations.Add(submitted.Value);
            }
        }

        distinctDestinations.Add(submittedToLocationId.Value);
        return distinctDestinations.Count > 1;
    }

    private async Task<bool> DetectHighFrequencyAsync(Guid assetId)
    {
        // Blocked attempts do not count as completed updates (Document 05).
        var windowStart = DateTime.UtcNow.AddMinutes(-_thresholds.HighUpdateFrequencyWindowMinutes);
        var recentCount = await _db.AssetMovements.CountAsync(m => m.AssetId == assetId && m.CreatedAt >= windowStart);
        return recentCount >= _thresholds.HighUpdateFrequencyCount;
    }

    private static Guid? TryGetAssetId(string? json) => TryGetGuidProperty(json, "AssetId");

    private static Guid? TryGetGuidProperty(string? json, string propertyName)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty(propertyName, out var prop) &&
                prop.ValueKind == JsonValueKind.String &&
                Guid.TryParse(prop.GetString(), out var value))
            {
                return value;
            }
        }
        catch (JsonException)
        {
            // Malformed metadata is ignored for detection purposes.
        }

        return null;
    }

    private async Task RecordBlockedAttemptAsync(
        Asset asset, MovementSourceType sourceType, Guid actingUserId, string reasonCode, string reasonMessage,
        Guid? submittedToLocationId, bool isSuspicious = false, string? suspiciousReason = null)
    {
        _auditLog.Log(actingUserId, AuditActions.MovementBlocked, AuditEntityTypes.Asset, asset.Id,
            new { AssetId = asset.Id, SourceType = sourceType.ToString(), ReasonCode = reasonCode, ReasonMessage = reasonMessage, SubmittedToLocationId = submittedToLocationId },
            isSuspicious, suspiciousReason);

        await _db.SaveChangesAsync();
    }

    internal static AssetMovementResponse MapMovementToResponse(AssetMovement movement, UserRole requesterRole)
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
