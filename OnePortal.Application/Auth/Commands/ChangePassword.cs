using FluentValidation;
using MediatR;
using OnePortal.Application.Abstractions;
using OnePortal.Domain.Entities;


namespace OnePortal.Application.Auth.Commands;

public record ChangePasswordCommand(string Email, string CurrentPassword, string NewPassword) : IRequest<bool>;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8);
    }
}

public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly IUserRepository _users;
    private readonly IPasswordService _pwd;
    private readonly ICurrentUser _current;
    private readonly IAuditLogRepository _audit;
    private readonly IUnitOfWork _uow;
    public ChangePasswordHandler(IUserRepository user, IPasswordService pwd, ICurrentUser current, IAuditLogRepository audit, IUnitOfWork uow)
    {  _users = user; _pwd = pwd; _current = current; _audit = audit; _uow = uow; }

    public async Task<bool> Handle(ChangePasswordCommand r, CancellationToken ct)
    {
        //var userId = _current.UserId ?? throw new UnauthorizedAccessException();
        var user = await _uow.Users.GetByEmailAsync(r.Email, ct);
        //var user = await _users.GetByIdAsync(userId, ct);
        if (user is null) return false;

        //if (!_pwd.Verify(user.PasswordHash, r.CurrentPassword))
        //    throw new UnauthorizedAccessException("Invalid current password");
        if (string.IsNullOrWhiteSpace(user.PasswordHash) || !_pwd.Verify(user.PasswordHash, r.CurrentPassword))
        {
            // bump lockout counters like normal invalid login
            user.AccessFailedCount++;
            if (user.AccessFailedCount >= 5) user.LockoutEndUtc = DateTime.UtcNow.AddMinutes(15);
            await _uow.SaveChangesAsync(ct);
            return false;
        }

        //user.PasswordHash = _pwd.Hash(r.NewPassword);
        //user.PasswordLastChangedUtc = DateTime.UtcNow;
        //user.MustChangePassword = false;

        //await _users.UpdateAsync(user, ct);
        ////await _audit.LogAsync(user.Id, "auth.password.changed", new { user.Id }, ct);
        //await _uow.SaveChangesAsync(ct);
        if (!user.MustChangePassword)
            throw new InvalidOperationException("Password change not required.");

        user.PasswordHash = _pwd.Hash(r.NewPassword);
        user.PasswordLastChangedUtc = DateTime.UtcNow;
        user.MustChangePassword = false;

        // reset lockout counters
        user.AccessFailedCount = 0;
        user.LockoutEndUtc = null;

        await _uow.SaveChangesAsync(ct);
        return true;
        
    }
}
