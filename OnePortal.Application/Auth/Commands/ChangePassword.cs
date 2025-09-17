using FluentValidation;
using MediatR;
using OnePortal.Application.Abstractions;
using OnePortal.Domain.Entities;


namespace OnePortal.Application.Auth.Commands;

public record ChangePasswordCommand(string CurrentPassword, string NewPassword) : IRequest<bool>;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
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
        var userId = _current.UserId ?? throw new UnauthorizedAccessException();
        var user = await _users.GetByIdAsync(userId, ct);

        if (!_pwd.Verify(user.PasswordHash, r.CurrentPassword))
            throw new UnauthorizedAccessException("Invalid current password");

        user.PasswordHash = _pwd.Hash(r.NewPassword);
        user.PasswordLastChangedUtc = DateTime.UtcNow;
        user.MustChangePassword = false;

        await _users.UpdateAsync(user, ct);
        //await _audit.LogAsync(user.Id, "auth.password.changed", new { user.Id }, ct);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}
