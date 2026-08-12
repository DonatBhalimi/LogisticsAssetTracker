using LogisticsAssetTracker.Api.Common;
using LogisticsAssetTracker.Api.Dtos.Approvals;
using LogisticsAssetTracker.Api.Dtos.Movements;

namespace LogisticsAssetTracker.Api.Services.Interfaces;

public interface IMovementApprovalService
{
    Task<PagedResult<MovementApprovalResponse>> ListAsync(MovementApprovalListQuery query);

    Task<MovementResult> ApproveAsync(Guid approvalId, ApprovalDecisionRequest request, Guid actingUserId);

    Task<MovementApprovalResponse> RejectAsync(Guid approvalId, ApprovalDecisionRequest request, Guid actingUserId);
}
