using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnePortal.Application.WebAuthn.Commands;
using OnePortal.Application.WebAuthn.Queries;

namespace OnePortal.API.Controllers;

[ApiController]
[Route("api/account/webauthn")]
[Authorize]
public class AccountWebAuthnController : ControllerBase
{
    private readonly IMediator _mediator;
    public AccountWebAuthnController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
        => Ok(await _mediator.Send(new ListCredentialsQuery(), ct));

    [HttpDelete("{credentialId}")]
    public async Task<IActionResult> Remove(string credentialId, CancellationToken ct)
    {
        var ok = await _mediator.Send(new RemoveCredentialCommand(credentialId), ct);
        return ok ? NoContent() : NotFound();
    }
}
