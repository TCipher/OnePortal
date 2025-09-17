using System.Text.Json;
using FluentValidation;
using MediatR;
using OnePortal.Application.Abstractions;
using OnePortal.Domain.Entities;

namespace OnePortal.Application.WebAuthn.Commands;

public record FinishRegistrationCommand(object AttestationResponse) : IRequest<object>;

public class FinishRegistrationValidator : AbstractValidator<FinishRegistrationCommand>
{
    public FinishRegistrationValidator() =>
        RuleFor(x => x.AttestationResponse).NotNull();
}

public class FinishRegistrationHandler : IRequestHandler<FinishRegistrationCommand, object>
{
    private readonly ICurrentUser _current;
    private readonly IChallengeStore _challenge;
    private readonly IWebAuthnProvider _provider;
    private readonly IWebAuthnCredentialRepository _creds;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;

    public FinishRegistrationHandler(
        ICurrentUser current,
        IChallengeStore challenge,
        IWebAuthnProvider provider,
        IWebAuthnCredentialRepository creds,
        IUserRepository users,
        IUnitOfWork uow)
    {
        _current = current;
        _challenge = challenge;
        _provider = provider;
        _creds = creds;
        _users = users;
        _uow = uow;
    }

    public async Task<object> Handle(FinishRegistrationCommand r, CancellationToken ct)
    {
        var userId = _current.UserId ?? throw new UnauthorizedAccessException();

        // 1. Get stored challenge
        var optionsJson = await _challenge.TakeAsync($"webauthn:attestation:{userId}", ct)
                         ?? throw new InvalidOperationException("Attestation options missing/expired");

        // 2. Verify response
        var attJson = JsonSerializer.Serialize(r.AttestationResponse);
        var res = await _provider.VerifyAttestationAsync(attJson, optionsJson, ct);

        // 3. Create credential
        var cred = new WebAuthnCredential
        {
            UserId = userId,
            CredentialId = res.CredentialId,
            PublicKey = res.PublicKey,
            SignCount = res.SignCount,
            Aaguid = Guid.TryParse(res.Aaguid, out var g) ? g : Guid.Empty,
            UserHandle = res.UserHandle,
            TransportsCsv = string.Join(",", res.Transports ?? Array.Empty<string>()),
            AttestationFmt = res.AttestationFormat,
            CreatedUtc = DateTime.UtcNow,
            IsActive = true
        };

        await _creds.AddAsync(cred, ct);

        // 4. Update user MFA enrollment
        var user = await _users.GetByIdAsync(userId, ct)
                   ?? throw new UnauthorizedAccessException();
        user.PreferredMfaMethod = MfaMethod.WebAuthn;
        user.MfaEnrolledUtc = DateTime.UtcNow;

        await _users.UpdateAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        // 5. Return client-friendly payload
        return new
        {
            credentialId = res.CredentialId,
            aaguid = res.Aaguid,
            createdUtc = cred.CreatedUtc
        };
    }
}
