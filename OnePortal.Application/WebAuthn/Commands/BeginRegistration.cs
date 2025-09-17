using System.Text.Json;
using FluentValidation;
using MediatR;
using OnePortal.Application.Abstractions;
using OnePortal.Domain.Entities;

namespace OnePortal.Application.WebAuthn.Commands;

public record BeginRegistrationCommand(string DisplayName) : IRequest<object>;

public class BeginRegistrationValidator : AbstractValidator<BeginRegistrationCommand>
{
    public BeginRegistrationValidator() =>
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(128);
}

public class BeginRegistrationHandler : IRequestHandler<BeginRegistrationCommand, object>
{
    private readonly ICurrentUser _current;
    private readonly IChallengeStore _challenge;
    private readonly IUserRepository _users;
    private readonly IWebAuthnCredentialRepository _creds;
    private readonly IWebAuthnProvider _provider;

    public BeginRegistrationHandler(
        ICurrentUser current,
        IChallengeStore challenge,
        IUserRepository users,
        IWebAuthnCredentialRepository creds,
        IWebAuthnProvider provider)
    {
        _current = current;
        _challenge = challenge;
        _users = users;
        _creds = creds;
        _provider = provider;
    }

    public async Task<object> Handle(BeginRegistrationCommand r, CancellationToken ct)
    {
        var userId = _current.UserId ?? throw new UnauthorizedAccessException();

        var user = await _users.GetByIdAsync(userId, ct)
                   ?? throw new UnauthorizedAccessException();

        var existing = await _creds.GetActiveByUserAsync(userId, ct);
        var excludeIds = existing.Select(c => c.CredentialId).ToList();

        var optionsJson = await _provider.BuildRegistrationOptionsAsync(
            userId,
            user.EmailAddress,
            r.DisplayName,
            excludeIds,
            ct);

        await _challenge.StoreAsync(
            $"webauthn:attestation:{userId}",
            optionsJson,
            TimeSpan.FromMinutes(10),
            ct);

        return JsonSerializer.Deserialize<object>(optionsJson)!;
    }
}
