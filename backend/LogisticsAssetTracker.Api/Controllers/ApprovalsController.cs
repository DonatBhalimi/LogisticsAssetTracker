using System.Security.Claims;
using FluentValidation;
using LogisticsAssetTracker.Api.Domain.Enums;
using LogisticsAssetTracker.Api.Dtos.Approvals;
using LogisticsAssetTracker.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogisticsAssetTracker.Api.Controllers;

[ApiController]
[Route("api/approvals")]
[Authorize(Roles = "Admin,Manager")]
public class ApprovalsController : ControllerBase
{
    private readonly IMovementApprovalService _approvalService;
    private readonly IValidator<ApprovalDecisionRequest> _decisionValidator;

    public ApprovalsController(IMovementApprovalService approvalService, IValidator<ApprovalDecisionRequest> decisionValidator)
    {
        _approvalService = approvalService;
        _decisionValidator = decisionValidator;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] MovementApprovalListQuery query)
    {
        var result = await _approvalService.ListAsync(query);
        return Ok(result);
    }

    [HttpGet("pending")]
    public async Task<IActionResult> Pending([FromQuery] MovementApprovalListQuery query)
    {
        query.Status = ApprovalStatus.Pending;
        var result = await _approvalService.ListAsync(query);
        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApprovalDecisionRequest request)
    {
        await _decisionValidator.ValidateAndThrowAsync(request);
        var result = await _approvalService.ApproveAsync(id, request, CurrentUserId());
        return Ok(result);
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] ApprovalDecisionRequest request)
    {
        await _decisionValidator.ValidateAndThrowAsync(request);
        var result = await _approvalService.RejectAsync(id, request, CurrentUserId());
        return Ok(result);
    }

    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
