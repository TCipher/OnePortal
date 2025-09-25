using System.Text.Json;
using FluentValidation;
using MediatR;
using OnePortal.Application.Abstractions;
using OnePortal.Domain.Entities;

namespace OnePortal.Application.WebAuthn.Commands;

public record BeginAssertionCommand(string Email) : IRequest<object>;

public class BeginAssertionValidator : AbstractValidator<BeginAssertionCommand>
{
    public BeginAssertionValidator() => RuleFor(x => x.Email).NotEmpty().EmailAddress();
}

public class BeginAssertionHandler : IRequestHandler<BeginAssertionCommand, object>
{
    private readonly IUserRepository _users;
    private readonly IWebAuthnCredentialRepository _creds;
    private readonly IChallengeStore _challenge;
    private readonly IWebAuthnProvider _provider;

    public BeginAssertionHandler(
        IUserRepository users,
        IWebAuthnCredentialRepository creds,
        IChallengeStore challenge,
        IWebAuthnProvider provider)
    {
        _users = users;
        _creds = creds;
        _challenge = challenge;
        _provider = provider;
    }

    public async Task<object> Handle(BeginAssertionCommand r, CancellationToken ct)
    {
        Console.WriteLine($"[BeginAssertionHandler] Starting assertion for email: {r.Email}");
        
        // 1. Lookup active user
        var user = await _users.GetByEmailAsync(r.Email, ct)
                   ?? throw new UnauthorizedAccessException();

        Console.WriteLine($"[BeginAssertionHandler] Found user: {user.EmailAddress}, Active: {user.IsActive}");

        if (!user.IsActive) throw new UnauthorizedAccessException();

        // 2. Get user's active credentials
        var creds = await _creds.GetActiveByUserAsync(user.Id, ct);
        var allowIds = creds.Select(c => c.CredentialId).ToList();
        
        Console.WriteLine($"[BeginAssertionHandler] Found {allowIds.Count} active credentials for user");

        // 3. Build WebAuthn assertion options
        var optionsJson = await _provider.BuildAssertionOptionsAsync(allowIds, ct);
        
        Console.WriteLine($"[BeginAssertionHandler] Got options JSON: {optionsJson != null}");
        Console.WriteLine($"[BeginAssertionHandler] Options JSON length: {optionsJson?.Length ?? 0}");

        // 4. Store challenge for verification step
        if (optionsJson == null) throw new InvalidOperationException("Failed to generate assertion options");
        await _challenge.StoreAsync(
            $"webauthn:assertion:{r.Email}",
            optionsJson,
            TimeSpan.FromMinutes(10),
            ct);

        // 5. Return deserialized options
        var result = JsonSerializer.Deserialize<object>(optionsJson);
        Console.WriteLine($"[BeginAssertionHandler] Deserialized result: {result != null}");
        
        return result!;
    }
}
