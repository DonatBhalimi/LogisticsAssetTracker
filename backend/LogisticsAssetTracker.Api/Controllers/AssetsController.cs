using System.Security.Claims;
using FluentValidation;
using LogisticsAssetTracker.Api.Domain.Enums;
using LogisticsAssetTracker.Api.Dtos.Assets;
using LogisticsAssetTracker.Api.Dtos.Movements;
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
    private readonly IAssetMovementService _movementService;
    private readonly IValidator<CreateAssetRequest> _createValidator;
    private readonly IValidator<UpdateAssetRequest> _updateValidator;
    private readonly IValidator<MovementRequest> _movementValidator;
    private readonly IValidator<ReactivateAssetRequest> _reactivateValidator;

    public AssetsController(
        IAssetService assetService,
        IAssetMovementService movementService,
        IValidator<CreateAssetRequest> createValidator,
        IValidator<UpdateAssetRequest> updateValidator,
        IValidator<MovementRequest> movementValidator,
        IValidator<ReactivateAssetRequest> reactivateValidator)
    {
        _assetService = assetService;
        _movementService = movementService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _movementValidator = movementValidator;
        _reactivateValidator = reactivateValidator;
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

    [HttpGet("by-code/{assetCode}")]
    public async Task<IActionResult> GetByCode(string assetCode)
    {
        var result = await _assetService.GetByCodeAsync(assetCode, CurrentUserRole());
        return Ok(result);
    }

    [HttpGet("qr/{qrCodeValue}")]
    public async Task<IActionResult> GetByQr(string qrCodeValue)
    {
        var result = await _assetService.GetByQrAsync(qrCodeValue, CurrentUserRole());
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

    [HttpGet("{id:guid}/movements")]
    public async Task<IActionResult> GetMovements(Guid id, [FromQuery] AssetMovementListQuery query)
    {
        var result = await _movementService.GetHistoryAsync(id, query, CurrentUserRole());
        return Ok(result);
    }

    [HttpPost("{id:guid}/movements/manual")]
    public async Task<IActionResult> CreateManualMovement(Guid id, [FromBody] MovementRequest request)
    {
        await _movementValidator.ValidateAndThrowAsync(request);
        var result = await _movementService.CreateManualMovementAsync(id, request, CurrentUserId(), CurrentUserRole());
        return MovementResultResponse(result);
    }

    [HttpPost("qr/{qrCodeValue}/movements")]
    public async Task<IActionResult> CreateQrMovement(string qrCodeValue, [FromBody] MovementRequest request)
    {
        await _movementValidator.ValidateAndThrowAsync(request);
        var result = await _movementService.CreateQrMovementAsync(qrCodeValue, request, CurrentUserId(), CurrentUserRole());
        return MovementResultResponse(result);
    }

    [HttpPost("{id:guid}/reactivate")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Reactivate(Guid id, [FromBody] ReactivateAssetRequest request)
    {
        await _reactivateValidator.ValidateAndThrowAsync(request);
        var result = await _movementService.ReactivateAsync(id, request, CurrentUserId(), CurrentUserRole());
        return StatusCode(StatusCodes.Status201Created, result);
    }

    // Document 06 movement outcomes: 201 for a completed movement, 202 when an approval was created instead.
    private IActionResult MovementResultResponse(MovementResult result) =>
        result.ResultType == "ApprovalRequired"
            ? StatusCode(StatusCodes.Status202Accepted, result)
            : StatusCode(StatusCodes.Status201Created, result);

    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private UserRole CurrentUserRole() => Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);
}
