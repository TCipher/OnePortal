using System.Text.Json;
using FluentValidation;
using MediatR;
using OnePortal.Application.Abstractions;
using OnePortal.Application.Auth.Dtos;
using OnePortal.Domain.Entities;

namespace OnePortal.Application.WebAuthn.Commands;

public record FinishAssertionCommand(string Email, object AssertionResponse) : IRequest<LoginResultDto>;

public class FinishAssertionValidator : AbstractValidator<FinishAssertionCommand>
{
    public FinishAssertionValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.AssertionResponse).NotNull();
    }
}

public class FinishAssertionHandler : IRequestHandler<FinishAssertionCommand, LoginResultDto>
{
    private readonly IUserRepository _users;
    private readonly IWebAuthnCredentialRepository _creds;
    private readonly IChallengeStore _challenge;
    private readonly IWebAuthnProvider _provider;
    private readonly ITokenService _tokens;
    private readonly IRefreshTokenService _refresh;
    private readonly IRepository<GlobalRole> _roles;

    public FinishAssertionHandler(
        IUserRepository users,
        IWebAuthnCredentialRepository creds,
        IChallengeStore challenge,
        IWebAuthnProvider provider,
        ITokenService tokens,
        IRefreshTokenService refresh,
        IRepository<GlobalRole> roles)
    {
        _users = users;
        _creds = creds;
        _challenge = challenge;
        _provider = provider;
        _tokens = tokens;
        _refresh = refresh;
        _roles = roles;
    }

    public async Task<LoginResultDto> Handle(FinishAssertionCommand r, CancellationToken ct)
    {
        // 1. Validate user
        var user = await _users.GetByEmailAsync(r.Email, ct)
                   ?? throw new UnauthorizedAccessException();

        if (!user.IsActive) throw new UnauthorizedAccessException("User inactive");

        // 2. Retrieve challenge
        var optionsJson = await _challenge.TakeAsync($"webauthn:assertion:{r.Email}", ct)
                         ?? throw new InvalidOperationException("Assertion options missing/expired");

        // 3. Extract credential id
        var assertionJson = JsonSerializer.Serialize(r.AssertionResponse);
        using var doc = JsonDocument.Parse(assertionJson);
        if (!doc.RootElement.TryGetProperty("id", out var idProp))
            throw new InvalidOperationException("Assertion missing credential id");
        var credentialId = idProp.GetString() ?? throw new InvalidOperationException("Invalid credential id");

        // 4. Lookup stored credential
        var cred = await _creds.GetActiveByCredentialIdForUserAsync(credentialId, user.Id, ct)
                   ?? throw new UnauthorizedAccessException();

        // 5. Verify assertion
        var newCounter = await _provider.VerifyAssertionAsync(
            assertionJson,
            optionsJson,
            credentialId,
            cred.PublicKey,
            cred.SignCount,
            cred.UserHandle,
            ct);

        // 6. Update sign counter
        cred.SignCount = newCounter;
        await _creds.UpdateAsync(cred, ct);

        // 7. Load global role
        var roleCode = (await _roles.GetByIdAsync(user.RoleId ?? 0, ct))?.Code ?? GlobalRoleCodes.User;
        var fullName = $"{user.FirstName} {(user.MiddleName ?? "")} {user.LastName}".Trim();

        // 8. Issue tokens
        var access = _tokens.CreateToken(user.Id, user.EmailAddress, fullName, roleCode);
        var (plaintext, _, _) = await _refresh.IssueAsync(user.Id, null, null, ct);

        return LoginResultDto.Issued(access, 1200, plaintext, 1200);
    }
}
