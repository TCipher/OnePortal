using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnePortal.Application.Common;
using OnePortal.Application.WebAuthn.Commands;
using OnePortal.Application.Auth.Dtos;

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
    public async Task<ActionResult<ApiResponse<LoginResultDto>>> FinishAssertion([FromBody] FinishAssertRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new FinishAssertionCommand(req.Email, req.AssertionResponse), ct);
        return Ok(ApiResponse<LoginResultDto>.Success(result));
    }

    // GitHub-style passkey authentication with automatic registration fallback
    [HttpPost("passkey/begin")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> BeginPasskeyAuth([FromBody] BeginPasskeyAuthRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new PasskeyAuthenticateWithFallbackCommand(req.Email, req.DisplayName), ct);
        
        if (result.Success && result.Options is not null)
        {
            return Ok(ApiResponse<object>.Success(new
            {
                options = result.Options,
                email = result.Email,
                isNewUser = result.IsNewUser,
                hasExistingPasskeys = result.HasExistingPasskeys
            }));
        }
        
        return BadRequest(ApiResponse<object>.Fail("passkey_error", result.Error ?? "Failed to initialize passkey authentication"));
    }

    [HttpPost("passkey/complete")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> CompletePasskeyAuth([FromBody] CompletePasskeyAuthRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new CompletePasskeyAuthCommand(req.Email, req.CredentialResponse), ct);
        
        if (result.Success && result.LoginResult is not null)
        {
            return Ok(ApiResponse<object>.Success(new
            {
                loginResult = result.LoginResult,
                message = result.Message
            }));
        }
        
        return BadRequest(ApiResponse<object>.Fail("passkey_error", result.Error ?? "Passkey authentication failed"));
    }
}

public record BeginRegRequest(string DisplayName);
public record BeginAssertRequest(string Email);
public record FinishAssertRequest(string Email, object AssertionResponse);
public record BeginPasskeyAuthRequest(string Email, string? DisplayName = null);
public record CompletePasskeyAuthRequest(string Email, object CredentialResponse);
