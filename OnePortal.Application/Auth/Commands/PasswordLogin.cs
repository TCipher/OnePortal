using FluentValidation;
using MediatR;
using OnePortal.Application.Abstractions;
using OnePortal.Application.Auth.Dtos;
using OnePortal.Domain.Entities;


namespace OnePortal.Application.Auth.Commands;

public record PasswordLoginCommand(string Email, string Password) : IRequest<LoginResultDto>;

public class PasswordLoginValidator : AbstractValidator<PasswordLoginCommand>
{
    public PasswordLoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class PasswordLoginHandler : IRequestHandler<PasswordLoginCommand, LoginResultDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordService _pwd;
    private readonly ITokenService _tokens;
    private readonly IRefreshTokenService _refresh;
    private readonly IUserRepository _users;
    public PasswordLoginHandler(IUnitOfWork uow, IPasswordService pwd, ITokenService tokens, IRefreshTokenService refresh, IUserRepository users)
    {
        _uow = uow;
        _pwd = pwd;
        _tokens = tokens;
        _refresh = refresh;
        _users = users;
    }

    public async Task<LoginResultDto> Handle(PasswordLoginCommand r, CancellationToken ct)
    {
        var user = await _users.GetByEmailAsync(r.Email, ct)
                  ?? throw new UnauthorizedAccessException("Invalid credentials");


        // Lockout logic
        if (user.LockoutEnabled && user.LockoutEndUtc is not null && user.LockoutEndUtc > DateTime.UtcNow)
            throw new UnauthorizedAccessException("Account locked");

        if (!_pwd.Verify(user.PasswordHash, r.Password))
        {
            user.AccessFailedCount++;
            if (user.AccessFailedCount >= 5) user.LockoutEndUtc = DateTime.UtcNow.AddMinutes(15);
            await _users.UpdateAsync(user, ct);
            await _uow.SaveChangesAsync(ct);
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        user.AccessFailedCount = 0;
        if (user.LockoutEndUtc is not null && user.LockoutEndUtc <= DateTime.UtcNow)
            user.LockoutEndUtc = null;
        await _users.UpdateAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        // Password expiry - set MustChangePassword if needed
        if (user.MustChangePassword || user.PasswordLastChangedUtc is null ||
            (DateTime.UtcNow - user.PasswordLastChangedUtc.Value).TotalDays >= 180)
        {
            user.MustChangePassword = true;
            await _users.UpdateAsync(user, ct);
            await _uow.SaveChangesAsync(ct);
            
            return new LoginResultDto
            {
                MustChangePassword = true,
                AccessToken = null,
                RefreshToken = null
            };
        }

        if (user.PreferredMfaMethod == MfaMethod.EmailOtp)
        {
            return new LoginResultDto
            {
                Mfa = new MfaStateDto
                {
                    Required = true,
                    Method = MfaMethod.EmailOtp.ToString(),  
                    ChallengeId = Guid.NewGuid().ToString("N")
                }
            };
        }


        // Issue tokens
        var roleCode = user.Role?.Code ?? GlobalRoleCodes.User;
        var fullName = string.Join(" ", new[] { user.FirstName, user.MiddleName, user.LastName }.Where(x => !string.IsNullOrWhiteSpace(x)));
        var access = _tokens.CreateToken(user.Id, user.EmailAddress, fullName, roleCode);
        var (plaintext, _, _) = await _refresh.IssueAsync(user.Id, null, null, ct);

        return LoginResultDto.Issued(access, 1200, plaintext, 1200);
    }
}

