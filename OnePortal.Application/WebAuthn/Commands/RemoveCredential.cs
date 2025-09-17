using FluentValidation;
using MediatR;
using OnePortal.Application.Abstractions;
using OnePortal.Domain.Entities;

namespace OnePortal.Application.WebAuthn.Commands;

public record RemoveCredentialCommand(string CredentialId) : IRequest<bool>;

public class RemoveCredentialValidator : AbstractValidator<RemoveCredentialCommand>
{
    public RemoveCredentialValidator() => RuleFor(x => x.CredentialId).NotEmpty();
}

public class RemoveCredentialHandler : IRequestHandler<RemoveCredentialCommand, bool>
{
    private readonly IWebAuthnCredentialRepository _creds;
    private readonly ICurrentUser _current;

    public RemoveCredentialHandler(IWebAuthnCredentialRepository creds, ICurrentUser current)
    {
        _creds = creds;
        _current = current;
    }

    public async Task<bool> Handle(RemoveCredentialCommand r, CancellationToken ct)
    {
        var userId = _current.UserId ?? throw new UnauthorizedAccessException();

        var cred = await _creds.GetActiveByCredentialIdForUserAsync(r.CredentialId, userId, ct);
        if (cred is null) return false;

        cred.IsActive = false;
        await _creds.UpdateAsync(cred, ct);

        return true;
    }
}
