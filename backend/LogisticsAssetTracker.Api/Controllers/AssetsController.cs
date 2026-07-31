using System.Security.Claims;
using FluentValidation;
using LogisticsAssetTracker.Api.Domain.Enums;
using LogisticsAssetTracker.Api.Dtos.Assets;
using LogisticsAssetTracker.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogisticsAssetTracker.Api.Controllers;

[ApiController]
[Route("api/assets")]
[Authorize]
public class AssetsController : ControllerBase
{
    private readonly IAssetService _assetService;
    private readonly IValidator<CreateAssetRequest> _createValidator;
    private readonly IValidator<UpdateAssetRequest> _updateValidator;

    public AssetsController(
        IAssetService assetService,
        IValidator<CreateAssetRequest> createValidator,
        IValidator<UpdateAssetRequest> updateValidator)
    {
        _assetService = assetService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] AssetListQuery query)
    {
        var result = await _assetService.ListAsync(query, CurrentUserRole());
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _assetService.GetByIdAsync(id, CurrentUserRole());
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateAssetRequest request)
    {
        await _createValidator.ValidateAndThrowAsync(request);
        var result = await _assetService.CreateAsync(request, CurrentUserId());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAssetRequest request)
    {
        await _updateValidator.ValidateAndThrowAsync(request);
        var result = await _assetService.UpdateAsync(id, request, CurrentUserId());
        return Ok(result);
    }

    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        await _assetService.DeactivateAsync(id, CurrentUserId());
        return NoContent();
    }

    [HttpGet("{id:guid}/qr")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetQr(Guid id)
    {
        var result = await _assetService.GetQrAsync(id);
        return Ok(result);
    }

    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private UserRole CurrentUserRole() => Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);
}
