using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using OnePortal.Application.Abstractions;
using OnePortal.Application.Auth.Commands;
using OnePortal.Application.Auth.Dtos;
using OnePortal.Application.Common;
using OnePortal.Domain.Entities;
using OnePortal.Infrastructure.Data;

namespace OnePortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokens;
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _uow;

    public AuthController(ITokenService tokens, IMediator mediator, IUnitOfWork uow)
    {
        _tokens = tokens;
        _mediator = mediator;
        _uow = uow;
    }

    //[HttpPost("dev-login")]
    //public async Task<IActionResult> DevLogin([FromBody] DevLoginRequest req, CancellationToken ct)
    //{
    //    var user = await _uow.Users.GetByEmailAsync(req.Email, ct);

    //    if (user is null) return Unauthorized("Unknown email.");

    //    var fullName = $"{user.FirstName} {(user.MiddleName ?? "")} {user.LastName}".Trim();
    //    var globalRoleCode = user.Role?.Code ?? GlobalRoleCodes.User;

    //    var token = _tokens.CreateToken(user.Id, user.EmailAddress, fullName, globalRoleCode);
    //    return Ok(new { token });
    //}

    //public record DevLoginRequest(string Email);

    [HttpPost("login")]
    [EnableRateLimiting("LoginLimiter")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResultDto>> Login([FromBody] PasswordLoginCommand cmd, CancellationToken ct)
    {
         var ok = await _mediator.Send(cmd, ct);
        return Ok(ApiResponse<LoginResultDto>.Success(ok));

    }
       

    [HttpPost("refresh")]
    [EnableRateLimiting("LoginLimiter")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResultDto>>> Refresh([FromBody] RefreshTokenCommand cmd, CancellationToken ct)
    {
        var ok = await _mediator.Send(cmd, ct);
        return Ok(ApiResponse<LoginResultDto>.Success(ok));
    }
        

    [HttpPost("otp/send")]
    [EnableRateLimiting("OtpLimiter")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> SendOtp([FromBody] SendEmailOtpCommand cmd, CancellationToken ct)
    {
        var res = await _mediator.Send(cmd, ct);
        return Ok(ApiResponse<object>.Success(new { res.Ok, expiresIn = res.ExpiresIn }));
    }

    [HttpPost("otp/verify")]
    [EnableRateLimiting("OtpLimiter")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResultDto>>> VerifyOtp([FromBody] VerifyEmailOtpCommand cmd, CancellationToken ct)

    {
         var res = await _mediator.Send(cmd, ct);
        return Ok(ApiResponse<LoginResultDto>.Success( res));

    } 
       

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> ForgotPassword([FromBody] ForgotPasswordCommand cmd, CancellationToken ct)
    {
        var ok = await _mediator.Send(cmd, ct);
        return Ok(ApiResponse<object>.Success(new { ok }));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword([FromBody] ResetPasswordCommand cmd, CancellationToken ct)
        {
        var ok = await _mediator.Send(cmd, ct);
        if (ok) return Ok(ApiResponse<object>.Success(new { ok = true }));
        return BadRequest(ApiResponse<object>.Fail("invalid_token", "Invalid or expired token."));
    }
}
