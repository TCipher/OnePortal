using System.Security.Cryptography;
using FluentValidation;
using MediatR;
using OnePortal.Application.Abstractions;
using OnePortal.Domain.Entities;

namespace OnePortal.Application.Auth.Commands;

public record ResetPasswordCommand(string Token, string NewPassword) : IRequest<bool>;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8);
    }
}

public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly IPasswordService _pwd;
    private readonly IUserRepository _users;
    private readonly IPasswordResetTokenRepository _tokens;
    private readonly IUnitOfWork _uow;
    //private readonly IAuditLogService? _audit;

   public ResetPasswordHandler(
      IPasswordService pwd,
       IUserRepository users,
        IPasswordResetTokenRepository tokens,
        IUnitOfWork uow 
       )
    { _pwd = pwd; _users = users; _tokens = tokens; _uow = uow; }

public async Task<bool> Handle(ResetPasswordCommand r, CancellationToken ct)
    {
        var tokenHash = Hash(r.Token);
        var resetToken = await _tokens.FindActiveByHashAsync(tokenHash, ct);
              if (resetToken is null || resetToken.ExpiresUtc < DateTime.UtcNow)
                       return false;

        var user = await _users.GetByIdAsync(resetToken.UserId, ct);
        if (user is null) return false;

        user.PasswordHash = _pwd.Hash(r.NewPassword);
        user.PasswordLastChangedUtc = DateTime.UtcNow;
        user.MustChangePassword = false;
        user.AccessFailedCount = 0;
        user.LockoutEndUtc = null;

        resetToken.UsedUtc = DateTime.UtcNow;
        await _users.UpdateAsync(user, ct);
        await _tokens.UpdateAsync(resetToken, ct);
        await _uow.SaveChangesAsync(ct);
        return true;
        
    }

    private static string Hash(string token)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token)));
    }
}
