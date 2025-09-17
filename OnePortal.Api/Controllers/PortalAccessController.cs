using MediatR;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnePortal.Application.Common;
using OnePortal.Application.PortalAccess.Commands;
using OnePortal.Application.PortalAccess.Dtos;
using OnePortal.Application.PortalAccess.Queries;
using OnePortal.Application.Users.Dtos;

namespace OnePortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PortalAccessController : ControllerBase
{
    private readonly IMediator _mediator;
    public PortalAccessController(IMediator mediator) => _mediator = mediator;

    // POST /api/portalaccess
    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserPortalAccessItemDto>>> Add([FromBody] AddUserToPortalCommand cmd, CancellationToken ct)
    {
        var result = await _mediator.Send(cmd, ct);
        return Ok(ApiResponse<UserPortalAccessItemDto>.Success(result));
    }

    // PUT /api/portalaccess/{userId}/{portalId}
    [HttpPut("{userId:int}/{portalId:int}")]
    public async Task<ActionResult<List<ApiResponse<UserPortalAccessItemDto>>>> Reassign(int userId, int portalId, [FromBody] ReassignPortalRoleCommand body, CancellationToken ct)
    {
        if (userId != body.UserId || portalId != body.PortalId) return BadRequest("Route/body mismatch.");
        var result = await _mediator.Send(body, ct);
        return Ok(ApiResponse<UserPortalAccessItemDto>.Success(result));
    }

    // PATCH /api/portalaccess/{userId}/{portalId}/deactivate
    [HttpPatch("{userId:int}/{portalId:int}/deactivate")]
    public async Task<ActionResult<ApiResponse<bool>>> Deactivate(int userId, int portalId, CancellationToken ct)
    {
        await _mediator.Send(new DeactivatePortalAccessCommand(userId, portalId), ct);
        return Ok(ApiResponse<bool>.Success(true));
    }

    // PATCH /api/portalaccess/{userId}/{portalId}/activate
    [HttpPatch("{userId:int}/{portalId:int}/activate")]
    public async Task<ActionResult<ApiResponse<bool>>> Activate(int userId, int portalId, CancellationToken ct)
    {
        await _mediator.Send(new ActivatePortalAccessCommand(userId, portalId), ct);
        return Ok(ApiResponse<bool>.Success(true));
    }

    // DELETE /api/portalaccess/{userId}/{portalId}
    [HttpDelete("{userId:int}/{portalId:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> Remove(int userId, int portalId, CancellationToken ct)
    {
        await _mediator.Send(new RemovePortalAccessCommand(userId, portalId), ct);
        return Ok(ApiResponse<bool>.Success(true));
    }

    // GET /api/portalaccess/user/{userId}
    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<List<ApiResponse<UserPortalAccessItemDto>>>> GetForUser(int userId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserPortalAccessesQuery(userId), ct);
        return Ok(ApiResponse<List<UserPortalAccessItemDto>>.Success(result));
    }

    // GET /api/portalaccess/portal/{portalId}
    [HttpGet("portal/{portalId:int}")]
    public async Task<ActionResult<List<ApiResponse<PortalUserItemDto>>>> GetUsersInPortal(int portalId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPortalUsersQuery(portalId), ct);
        return Ok(ApiResponse<List<PortalUserItemDto>>.Success(result));
    }
}
