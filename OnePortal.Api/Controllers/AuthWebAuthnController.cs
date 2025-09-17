using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnePortal.Application.Common;
using OnePortal.Application.WebAuthn.Commands;

namespace OnePortal.API.Controllers;

[ApiController]
[Route("api/auth/webauthn")]
public class AuthWebAuthnController : ControllerBase
{
    private readonly IMediator _mediator;
    public AuthWebAuthnController(IMediator mediator) => _mediator = mediator;

    // Registration (Auth required)
    [HttpPost("registration/begin")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> BeginRegistration([FromBody] BeginRegRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new BeginRegistrationCommand(req.DisplayName), ct);
        return Ok(ApiResponse<object>.Success(result));
    }

    [HttpPost("registration/finish")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> FinishRegistration([FromBody] object attestationResponse, CancellationToken ct)
    {
        var result = await _mediator.Send(new FinishRegistrationCommand(attestationResponse), ct);
        return StatusCode(201, ApiResponse<object>.Success(result));
    }

    // Assertion (Passwordless login)
    [HttpPost("assertion/begin")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> BeginAssertion([FromBody] BeginAssertRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new BeginAssertionCommand(req.Email), ct);
        return Ok(ApiResponse<object>.Success(result));
    }

    [HttpPost("assertion/finish")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> FinishAssertion([FromBody] FinishAssertRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new FinishAssertionCommand(req.Email, req.AssertionResponse), ct);
        return Ok(ApiResponse<object>.Success(result));
    }
}

public record BeginRegRequest(string DisplayName);
public record BeginAssertRequest(string Email);
public record FinishAssertRequest(string Email, object AssertionResponse);
