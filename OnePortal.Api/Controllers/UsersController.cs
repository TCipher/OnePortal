// OnePortal.API/Controllers/UsersController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnePortal.Application.Common;
using OnePortal.Application.Users.Commands;
using OnePortal.Application.Users.Dtos;
using OnePortal.Application.Users.Queries;

namespace OnePortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    public UsersController(IMediator mediator) => _mediator = mediator;

    // POST /api/users
    [HttpPost]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<ActionResult<ApiResponse<UserDto>>>Create([FromBody] CreateUserCommand cmd, CancellationToken ct)
    {
        var user = await _mediator.Send(cmd, ct);
        return Ok(ApiResponse<UserDto>.Success(user));
    }

    // PUT /api/users/{id}
    [HttpPut("{id:int}")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<ActionResult<UserDto>> Update(int id, [FromBody] UpdateUserCommand body, CancellationToken ct)
    {
        if (id != body.Id)
            return BadRequest(ApiResponse<string>.Fail("400", "Id mismatch."));

        var result = await _mediator.Send(body, ct);

        return Ok(ApiResponse<UserDto>.Success(result));
    }


    // DELETE /api/users/{id} (soft delete)
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> SoftDelete(int id, CancellationToken ct)
    {
        var ok = await _mediator.Send(new SoftDeleteUserCommand(id), ct);
        return Ok(ApiResponse<bool>.Success(ok));
    }

    // GET /api/users/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById([FromRoute] int id, CancellationToken ct)
    {
        var user = await _mediator.Send(new GetUserByIdQuery(id), ct);
        if (user is null)
            return NotFound(ApiResponse<UserDto>.Fail("not_found", "User not found"));
        return Ok(ApiResponse<UserDto>.Success(user));
    }

    // GET /api/users?page=1&pageSize=20
    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetUsersPagedQuery(page, pageSize), ct);
        return Ok(ApiResponse<object>.Success(result));
    }
}
