using FluentValidation;
using MediatR;
using OnePortal.Application.Auth.Dtos;
using OnePortal.Application.Abstractions;
using OnePortal.Domain.Entities;

namespace OnePortal.Application.Auth.Commands;

public record VerifyEmailOtpCommand(string Email, string Otp, string? ChallengeId) : IRequest<LoginResultDto>;

public class VerifyEmailOtpValidator : AbstractValidator<VerifyEmailOtpCommand>
{
    public VerifyEmailOtpValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Otp).Length(6).Matches("^[0-9]{6}$");
    }
}

public class VerifyEmailOtpHandler : IRequestHandler<VerifyEmailOtpCommand, LoginResultDto>
{
    private readonly IOtpService _otp;
    private readonly IUnitOfWork _uow;
    private readonly ITokenService _tokens;
    private readonly IRefreshTokenService _refresh;

    public VerifyEmailOtpHandler(IOtpService otp, IUnitOfWork uow, ITokenService tokens, IRefreshTokenService refresh)
    { _otp = otp; _uow = uow; _tokens = tokens; _refresh = refresh; }

    public async Task<LoginResultDto> Handle(VerifyEmailOtpCommand r, CancellationToken ct)
    {
        var ok = await _otp.VerifyEmailOtpAsync(r.Email, r.Otp, r.ChallengeId, ct);
        if (!ok) throw new UnauthorizedAccessException("Invalid/expired OTP");

        var user = await _uow.Users.GetByEmailAsync(r.Email, ct);
        if (user == null) throw new UnauthorizedAccessException("User not found");

        var roleCode = user.Role?.Code ?? GlobalRoleCodes.User;
        var fullName = $"{user.FirstName} {(user.MiddleName ?? "")} {user.LastName}".Trim();
        var access = _tokens.CreateToken(user.Id, user.EmailAddress, fullName, roleCode);
        var (plaintext, _, _) = await _refresh.IssueAsync(user.Id, null, null, ct);

        return LoginResultDto.Issued(access, 1200, plaintext, 1200);
    }
}
