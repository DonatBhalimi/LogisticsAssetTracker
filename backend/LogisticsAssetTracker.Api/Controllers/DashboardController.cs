using LogisticsAssetTracker.Api.Dtos.Movements;
using LogisticsAssetTracker.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogisticsAssetTracker.Api.Controllers;

// Document 06 Dashboard Endpoints. Suspicious-activity and pending-approvals sections
// are intentionally not duplicated here — the frontend reuses the existing
// GET /api/audit-logs?isSuspicious=true and GET /api/approvals/pending endpoints
// (Phase 4 data), per the approved Phase 5 scope to reuse existing endpoints where one
// already exists rather than building near-duplicates.
[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "Admin,Manager")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary()
    {
        var result = await _dashboardService.GetSummaryAsync();
        return Ok(result);
    }

    [HttpGet("assets-by-status")]
    public async Task<IActionResult> AssetsByStatus()
    {
        var result = await _dashboardService.GetAssetsByStatusAsync();
        return Ok(result);
    }

    [HttpGet("assets-by-condition")]
    public async Task<IActionResult> AssetsByCondition()
    {
        var result = await _dashboardService.GetAssetsByConditionAsync();
        return Ok(result);
    }

    [HttpGet("assets-by-location")]
    public async Task<IActionResult> AssetsByLocation()
    {
        var result = await _dashboardService.GetAssetsByLocationAsync();
        return Ok(result);
    }

    [HttpGet("recent-movements")]
    public async Task<IActionResult> RecentMovements([FromQuery] AssetMovementListQuery query)
    {
        var result = await _dashboardService.GetRecentMovementsAsync(query);
        return Ok(result);
    }
}
