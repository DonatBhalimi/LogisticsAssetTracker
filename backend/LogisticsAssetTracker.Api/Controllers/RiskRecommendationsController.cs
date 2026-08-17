using LogisticsAssetTracker.Api.Dtos.Risk;
using LogisticsAssetTracker.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogisticsAssetTracker.Api.Controllers;

[ApiController]
[Route("api/risk-recommendations")]
[Authorize(Roles = "Admin,Manager")]
public class RiskRecommendationsController : ControllerBase
{
    private readonly IRiskAnalysisService _riskAnalysisService;

    public RiskRecommendationsController(IRiskAnalysisService riskAnalysisService)
    {
        _riskAnalysisService = riskAnalysisService;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] RiskRecommendationListQuery query)
    {
        var result = await _riskAnalysisService.ListAsync(query);
        return Ok(result);
    }
}
