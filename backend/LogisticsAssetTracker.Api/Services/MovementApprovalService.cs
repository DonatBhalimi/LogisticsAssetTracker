using LogisticsAssetTracker.Api.Common;
using LogisticsAssetTracker.Api.Data;
using LogisticsAssetTracker.Api.Domain.Entities;
using LogisticsAssetTracker.Api.Domain.Enums;
using LogisticsAssetTracker.Api.Dtos.Approvals;
using LogisticsAssetTracker.Api.Dtos.Movements;
using LogisticsAssetTracker.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LogisticsAssetTracker.Api.Services;

// Manager/Admin approval decisions on Operator Lost-to-Available requests (Document 05).
public class MovementApprovalService : IMovementApprovalService
{
    private readonly AppDbContext _db;
    private readonly IAuditLogService _auditLog;

    public MovementApprovalService(AppDbContext db, IAuditLogService auditLog)
    {
        _db = db;
        _auditLog = auditLog;
    }

    public async Task<PagedResult<MovementApprovalResponse>> ListAsync(MovementApprovalListQuery query)
    {
        var approvals = _db.MovementApprovals
            .Include(a => a.Asset)
            .Include(a => a.RequestedByUser)
            .Include(a => a.DecidedByUser)
            .Include(a => a.RequestedLocation)
            .AsQueryable();

        if (query.Status.HasValue)
        {
            approvals = approvals.Where(a => a.ApprovalStatus == query.Status.Value);
        }

        if (query.AssetId.HasValue)
        {
            approvals = approvals.Where(a => a.AssetId == query.AssetId.Value);
        }

        if (query.RequestedByUserId.HasValue)
        {
            approvals = approvals.Where(a => a.RequestedByUserId == query.RequestedByUserId.Value);
        }

        if (query.FromDate.HasValue)
        {
            approvals = approvals.Where(a => a.CreatedAt >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            approvals = approvals.Where(a => a.CreatedAt <= query.ToDate.Value);
        }

        approvals = query.SortDirection?.ToLowerInvariant() == "asc"
            ? approvals.OrderBy(a => a.CreatedAt)
            : approvals.OrderByDescending(a => a.CreatedAt);

        var totalCount = await approvals.CountAsync();
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

        var items = await approvals
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<MovementApprovalResponse>
        {
            Items = items.Select(a => MapToResponse(a, a.Asset!)).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<MovementResult> ApproveAsync(Guid approvalId, ApprovalDecisionRequest request, Guid actingUserId)
    {
        var approval = await _db.MovementApprovals
            .Include(a => a.Asset)
            .ThenInclude(a => a!.CurrentLocation)
            .Include(a => a.RequestedLocation)
            .FirstOrDefaultAsync(a => a.Id == approvalId)
            ?? throw ApiException.NotFound("APPROVAL_NOT_FOUND", "Approval not found.");

        var decisionNote = request.DecisionNote.Trim();
        var asset = approval.Asset!;

        if (approval.ApprovalStatus != ApprovalStatus.Pending)
        {
            await RecordBlockedDecisionAsync(approval, actingUserId, "STALE_APPROVAL", "Approval must be Pending.");
            throw ApiException.Conflict("STALE_APPROVAL", "Approval is no longer pending.");
        }

        // Revalidate the asset and requested destination before applying (Document 05).
        if (!asset.IsActive || asset.Status != AssetStatus.Lost || asset.Condition != AssetCondition.Good)
        {
            await RecordBlockedDecisionAsync(approval, actingUserId, "STALE_APPROVAL", "Asset state no longer matches the approval request.");
            throw ApiException.Conflict("STALE_APPROVAL", "Asset state no longer matches the approval request.");
        }

        Location? destination = asset.CurrentLocation;
        var toLocationId = asset.CurrentLocationId;
        if (approval.RequestedLocationId.HasValue)
        {
            destination = await _db.Locations.FindAsync(approval.RequestedLocationId.Value)
                ?? throw ApiException.NotFound("LOCATION_NOT_FOUND", "Location not found.");

            if (!destination.IsActive)
            {
                await RecordBlockedDecisionAsync(approval, actingUserId, "STALE_APPROVAL", "Requested destination is no longer active.");
                throw ApiException.Conflict("STALE_APPROVAL", "Requested destination is no longer active.");
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
            NewCondition = AssetCondition.Good,
            SourceType = approval.RequestedSourceType,
            UpdatedByUserId = actingUserId,
            Notes = approval.RequestReason,
            IsSuspicious = false,
            SuspiciousReason = null,
            CreatedAt = DateTime.UtcNow
        };

        _db.AssetMovements.Add(movement);

        asset.CurrentLocationId = toLocationId;
        asset.CurrentLocation = destination;
        asset.Status = AssetStatus.Available;
        asset.Condition = AssetCondition.Good;
        asset.UpdatedAt = DateTime.UtcNow;

        approval.ApprovalStatus = ApprovalStatus.Approved;
        approval.DecidedByUserId = actingUserId;
        approval.DecisionNote = decisionNote;
        approval.DecidedAt = DateTime.UtcNow;

        _auditLog.Log(actingUserId, AuditActions.MovementApprovalApproved, AuditEntityTypes.MovementApproval, approval.Id,
            new { AssetId = asset.Id, ApprovalId = approval.Id, DecisionNote = decisionNote });

        await _db.SaveChangesAsync();

        movement.FromLocation = previousLocation;
        movement.ToLocation = destination;
        movement.UpdatedByUser = await _db.Users.FindAsync(actingUserId);

        return new MovementResult
        {
            ResultType = "MovementCreated",
            Movement = AssetMovementService.MapMovementToResponse(movement, UserRole.Manager),
            Asset = AssetService.MapToResponse(asset)
        };
    }

    public async Task<MovementApprovalResponse> RejectAsync(Guid approvalId, ApprovalDecisionRequest request, Guid actingUserId)
    {
        var approval = await _db.MovementApprovals
            .Include(a => a.Asset)
            .Include(a => a.RequestedByUser)
            .Include(a => a.RequestedLocation)
            .FirstOrDefaultAsync(a => a.Id == approvalId)
            ?? throw ApiException.NotFound("APPROVAL_NOT_FOUND", "Approval not found.");

        var decisionNote = request.DecisionNote.Trim();

        if (approval.ApprovalStatus != ApprovalStatus.Pending)
        {
            await RecordBlockedDecisionAsync(approval, actingUserId, "STALE_APPROVAL", "Approval must be Pending.");
            throw ApiException.Conflict("STALE_APPROVAL", "Approval is no longer pending.");
        }

        approval.ApprovalStatus = ApprovalStatus.Rejected;
        approval.DecidedByUserId = actingUserId;
        approval.DecisionNote = decisionNote;
        approval.DecidedAt = DateTime.UtcNow;

        _auditLog.Log(actingUserId, AuditActions.MovementApprovalRejected, AuditEntityTypes.MovementApproval, approval.Id,
            new { AssetId = approval.AssetId, ApprovalId = approval.Id, DecisionNote = decisionNote });

        await _db.SaveChangesAsync();

        approval.DecidedByUser = await _db.Users.FindAsync(actingUserId);

        return MapToResponse(approval, approval.Asset!);
    }

    private async Task RecordBlockedDecisionAsync(MovementApproval approval, Guid actingUserId, string reasonCode, string reasonMessage)
    {
        _auditLog.Log(actingUserId, AuditActions.MovementBlocked, AuditEntityTypes.MovementApproval, approval.Id,
            new { AssetId = approval.AssetId, ApprovalId = approval.Id, ReasonCode = reasonCode, ReasonMessage = reasonMessage });

        await _db.SaveChangesAsync();
    }

    internal static MovementApprovalResponse MapToResponse(MovementApproval approval, Asset asset) => new()
    {
        Id = approval.Id,
        AssetId = approval.AssetId,
        AssetCode = asset.AssetCode,
        AssetName = asset.Name,
        RequestedByUserId = approval.RequestedByUserId,
        RequestedByUserName = approval.RequestedByUser?.FullName ?? string.Empty,
        DecidedByUserId = approval.DecidedByUserId,
        DecidedByUserName = approval.DecidedByUser?.FullName,
        RequestedLocationId = approval.RequestedLocationId,
        RequestedLocationName = approval.RequestedLocation?.Name,
        RequestedStatus = approval.RequestedStatus,
        RequestedCondition = approval.RequestedCondition,
        RequestedSourceType = approval.RequestedSourceType,
        ApprovalStatus = approval.ApprovalStatus,
        RequestReason = approval.RequestReason,
        DecisionNote = approval.DecisionNote,
        CreatedAt = approval.CreatedAt,
        DecidedAt = approval.DecidedAt
    };
}
