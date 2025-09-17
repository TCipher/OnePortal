using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnePortal.Application.AuditLogs.Queries;

namespace OnePortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOrSuperAdmin")] // view-only for Admin/SuperAdmin
public class AuditLogsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AuditLogsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetForUser(int userId, [FromQuery] int take = 200, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAuditLogsForUserQuery(userId, take), ct);
        return Ok(result);
    }

    [HttpGet("portal/{portalId:int}")]
    public async Task<IActionResult> GetForPortal(int portalId, [FromQuery] int take = 200, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAuditLogsForPortalQuery(portalId, take), ct);
        return Ok(result);
    }
}
