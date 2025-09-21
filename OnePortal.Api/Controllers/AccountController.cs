using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnePortal.Application.Auth.Commands;

namespace OnePortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class AccountController : ControllerBase
{
    private readonly IMediator _mediator;
    public AccountController(IMediator mediator) => _mediator = mediator;

    [HttpPost("change-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand cmd, CancellationToken ct)
    {
        var ok = await _mediator.Send(cmd, ct);
        return Ok(new { ok = ok, mustRelogin = true });
    }
}
