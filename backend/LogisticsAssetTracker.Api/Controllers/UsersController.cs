using System.Security.Claims;
using FluentValidation;
using LogisticsAssetTracker.Api.Dtos.Users;
using LogisticsAssetTracker.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogisticsAssetTracker.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IValidator<CreateUserRequest> _createValidator;
    private readonly IValidator<UpdateUserRequest> _updateValidator;

    public UsersController(
        IUserService userService,
        IValidator<CreateUserRequest> createValidator,
        IValidator<UpdateUserRequest> updateValidator)
    {
        _userService = userService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] UserListQuery query)
    {
        var result = await _userService.ListAsync(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        await _createValidator.ValidateAndThrowAsync(request);
        var result = await _userService.CreateAsync(request, CurrentUserId());
        return CreatedAtAction(nameof(List), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        await _updateValidator.ValidateAndThrowAsync(request);
        var result = await _userService.UpdateAsync(id, request, CurrentUserId());
        return Ok(result);
    }

    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
