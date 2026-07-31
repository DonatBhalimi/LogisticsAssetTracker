using System.Security.Claims;
using FluentValidation;
using LogisticsAssetTracker.Api.Domain.Enums;
using LogisticsAssetTracker.Api.Dtos.Locations;
using LogisticsAssetTracker.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogisticsAssetTracker.Api.Controllers;

[ApiController]
[Route("api/locations")]
[Authorize]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _locationService;
    private readonly IValidator<CreateLocationRequest> _createValidator;
    private readonly IValidator<UpdateLocationRequest> _updateValidator;

    public LocationsController(
        ILocationService locationService,
        IValidator<CreateLocationRequest> createValidator,
        IValidator<UpdateLocationRequest> updateValidator)
    {
        _locationService = locationService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] LocationListQuery query)
    {
        var result = await _locationService.ListAsync(query, CurrentUserRole());
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateLocationRequest request)
    {
        await _createValidator.ValidateAndThrowAsync(request);
        var result = await _locationService.CreateAsync(request, CurrentUserId());
        return CreatedAtAction(nameof(List), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLocationRequest request)
    {
        await _updateValidator.ValidateAndThrowAsync(request);
        var result = await _locationService.UpdateAsync(id, request, CurrentUserId());
        return Ok(result);
    }

    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        await _locationService.DeactivateAsync(id, CurrentUserId());
        return NoContent();
    }

    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private UserRole CurrentUserRole() => Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);
}
