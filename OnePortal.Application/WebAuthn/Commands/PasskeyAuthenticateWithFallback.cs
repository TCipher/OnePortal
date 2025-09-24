using System.Text.Json;
using FluentValidation;
using MediatR;
using OnePortal.Application.Abstractions;
using OnePortal.Application.Auth.Dtos;
using OnePortal.Domain.Entities;
using OnePortal.Application.Common;

namespace OnePortal.Application.WebAuthn.Commands;

/// <summary>
/// GitHub-style passkey authentication that automatically falls back to registration if no passkeys exist
/// </summary>
public record PasskeyAuthenticateWithFallbackCommand(string Email, string? DisplayName = null) : IRequest<PasskeyAuthResult>;

public class PasskeyAuthenticateWithFallbackValidator : AbstractValidator<PasskeyAuthenticateWithFallbackCommand>
{
    public PasskeyAuthenticateWithFallbackValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.DisplayName).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.DisplayName));
    }
}

public class PasskeyAuthenticateWithFallbackHandler : IRequestHandler<PasskeyAuthenticateWithFallbackCommand, PasskeyAuthResult>
{
    private readonly IUnitOfWork _uow;
    private readonly IChallengeStore _challenge;
    private readonly IWebAuthnProvider _provider;
    private readonly ITokenService _tokens;
    private readonly IRefreshTokenService _refresh;

    public PasskeyAuthenticateWithFallbackHandler(
        IUnitOfWork uow,
        IChallengeStore challenge,
        IWebAuthnProvider provider,
        ITokenService tokens,
        IRefreshTokenService refresh)
    {
        _uow = uow;
        _challenge = challenge;
        _provider = provider;
        _tokens = tokens;
        _refresh = refresh;
    }

    public async Task<PasskeyAuthResult> Handle(PasskeyAuthenticateWithFallbackCommand r, CancellationToken ct)
    {
        Console.WriteLine($"[PasskeyAuthenticateWithFallback] Starting authentication for email: {r.Email}");

        // 1. Check if user exists
        var user = await _uow.Users.GetByEmailAsync(r.Email, ct);
        var isNewUser = user == null;

        if (isNewUser)
        {
            Console.WriteLine($"[PasskeyAuthenticateWithFallback] User {r.Email} not found, will create new user");
        }
        else
        {
            Console.WriteLine($"[PasskeyAuthenticateWithFallback] Found existing user: {user.EmailAddress}, Active: {user.IsActive}");
            
            if (!user.IsActive)
            {
                return PasskeyAuthResult.Failed("Account is inactive");
            }
        }

        // 2. Get existing credentials (if user exists)
        var existingCreds = new List<string>();
        if (!isNewUser)
        {
            var creds = await _uow.WebAuthnCredentials.GetActiveByUserAsync(user!.Id, ct);
            existingCreds = creds.Select(c => c.CredentialId).ToList();
            Console.WriteLine($"[PasskeyAuthenticateWithFallback] Found {existingCreds.Count} existing credentials");
        }

        // 3. Build assertion options that work for both authentication and registration
        var optionsJson = await _provider.BuildUniversalPasskeyOptionsAsync(
            r.Email, 
            r.DisplayName ?? r.Email, 
            existingCreds, 
            ct);

        Console.WriteLine($"[PasskeyAuthenticateWithFallback] Generated universal options");

        // 4. Store challenge for verification
        var challengeKey = $"passkey:universal:{r.Email}";
        await _challenge.StoreAsync(challengeKey, optionsJson, TimeSpan.FromMinutes(10), ct);

        // 5. Return options that will work for both auth and registration
        var options = JsonSerializer.Deserialize<object>(optionsJson);
        
        return PasskeyAuthResult.OptionsReady(options!, r.Email, isNewUser, existingCreds.Count > 0);
    }
}

/// <summary>
/// Complete the passkey authentication/registration flow
/// </summary>
public record CompletePasskeyAuthCommand(string Email, object CredentialResponse) : IRequest<PasskeyAuthResult>;

public class CompletePasskeyAuthValidator : AbstractValidator<CompletePasskeyAuthCommand>
{
    public CompletePasskeyAuthValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.CredentialResponse).NotNull();
    }
}

public class CompletePasskeyAuthHandler : IRequestHandler<CompletePasskeyAuthCommand, PasskeyAuthResult>
{
    private readonly IUnitOfWork _uow;
    private readonly IChallengeStore _challenge;
    private readonly IWebAuthnProvider _provider;
    private readonly ITokenService _tokens;
    private readonly IRefreshTokenService _refresh;

    public CompletePasskeyAuthHandler(
        IUnitOfWork uow,
        IChallengeStore challenge,
        IWebAuthnProvider provider,
        ITokenService tokens,
        IRefreshTokenService refresh)
    {
        _uow = uow;
        _challenge = challenge;
        _provider = provider;
        _tokens = tokens;
        _refresh = refresh;
    }

    public async Task<PasskeyAuthResult> Handle(CompletePasskeyAuthCommand r, CancellationToken ct)
    {
        Console.WriteLine($"[CompletePasskeyAuth] Completing authentication for: {r.Email}");

        try
        {
            // 1. Retrieve stored options
            var challengeKey = $"passkey:universal:{r.Email}";
            var optionsJson = await _challenge.TakeAsync(challengeKey, ct)
                             ?? throw new InvalidOperationException("Passkey options missing/expired");

            // 2. Check if this is a registration or authentication
            var credentialJson = JsonSerializer.Serialize(r.CredentialResponse);
            using var doc = JsonDocument.Parse(credentialJson);
            
            if (!doc.RootElement.TryGetProperty("response", out var responseProp))
                throw new InvalidOperationException("Invalid credential response");

            // 3. Determine if this is registration (has attestationObject) or authentication
            var isRegistration = responseProp.TryGetProperty("attestationObject", out _);
            
            Console.WriteLine($"[CompletePasskeyAuth] Detected flow type: {(isRegistration ? "Registration" : "Authentication")}");

            if (isRegistration)
            {
                return await HandleRegistration(r.Email, credentialJson, optionsJson, ct);
            }
            else
            {
                return await HandleAuthentication(r.Email, credentialJson, optionsJson, ct);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[CompletePasskeyAuth] Error: {ex}");
            return PasskeyAuthResult.Failed($"Authentication failed: {ex.Message}");
        }
    }

    private async Task<PasskeyAuthResult> HandleRegistration(string email, string credentialJson, string optionsJson, CancellationToken ct)
    {
        Console.WriteLine($"[CompletePasskeyAuth] Handling registration for: {email}");

        // 1. Verify attestation
        var credentialResult = await _provider.VerifyAttestationAsync(credentialJson, optionsJson, ct);
        
        // 2. Create or get user
        var user = await _uow.Users.GetByEmailAsync(email, ct);
        var isNewUser = user == null;

        if (isNewUser)
        {
            // Create new user
            user = new UserDetails
            {
                EmailAddress = email,
                FirstName = email.Split('@')[0], // Simple name from email
                LastName = "",
                IsActive = true,
                PreferredMfaMethod = MfaMethod.WebAuthn, // Use WebAuthn for passkeys
                Createddate = DateTime.UtcNow,
                IsOneportalUser = true
            };
            
            await _uow.Users.AddAsync(user, ct);
            Console.WriteLine($"[CompletePasskeyAuth] Created new user: {user.Id}");
        }

        // 3. Store credential
        var credential = new WebAuthnCredential
        {
            UserId = user.Id,
            CredentialId = credentialResult.CredentialId,
            PublicKey = credentialResult.PublicKey,
            SignCount = credentialResult.SignCount,
            UserHandle = credentialResult.UserHandle,
            Aaguid = Guid.Parse(credentialResult.Aaguid ?? Guid.Empty.ToString()),
            AttestationFmt = credentialResult.AttestationFormat,
            TransportsCsv = string.Join(",", credentialResult.Transports),
            CreatedUtc = DateTime.UtcNow,
            IsActive = true
        };

        await _uow.WebAuthnCredentials.AddAsync(credential, ct);
        await _uow.SaveChangesAsync(ct);

        Console.WriteLine($"[CompletePasskeyAuth] Stored credential: {credential.CredentialId}");

        // 4. Issue tokens
        var roleCode = (await _uow.Repository<GlobalRole>().GetByIdAsync(user.RoleId ?? 0, ct))?.Code ?? GlobalRoleCodes.User;
        var fullName = $"{user.FirstName} {(user.MiddleName ?? "")} {user.LastName}".Trim();
        var access = _tokens.CreateToken(user.Id, user.EmailAddress, fullName, roleCode);
        var (plaintext, _, _) = await _refresh.IssueAsync(user.Id, null, null, ct);

        return PasskeyAuthResult.CreateSuccess(
            LoginResultDto.Issued(access, 1200, plaintext, 1200),
            isNewUser ? "Account created and signed in with passkey" : "Signed in with new passkey"
        );
    }

    private async Task<PasskeyAuthResult> HandleAuthentication(string email, string credentialJson, string optionsJson, CancellationToken ct)
    {
        Console.WriteLine($"[CompletePasskeyAuth] Handling authentication for: {email}");

        // 1. Get user
        var user = await _uow.Users.GetByEmailAsync(email, ct)
                  ?? throw new UnauthorizedAccessException("User not found");

        if (!user.IsActive) throw new UnauthorizedAccessException("Account inactive");

        // 2. Extract credential id
        using var doc = JsonDocument.Parse(credentialJson);
        if (!doc.RootElement.TryGetProperty("id", out var idProp))
            throw new InvalidOperationException("Credential missing id");
        
        var credentialId = idProp.GetString() ?? throw new InvalidOperationException("Invalid credential id");

        // 3. Get stored credential
        var cred = await _uow.WebAuthnCredentials.GetActiveByCredentialIdForUserAsync(credentialId, user.Id, ct)
                  ?? throw new UnauthorizedAccessException("Credential not found");

        // 4. Verify assertion
        var newCounter = await _provider.VerifyAssertionAsync(
            credentialJson,
            optionsJson,
            credentialId,
            cred.PublicKey,
            cred.SignCount,
            cred.UserHandle,
            ct);

        // 5. Update sign counter
        cred.SignCount = newCounter;
        await _uow.WebAuthnCredentials.UpdateAsync(cred, ct);
        await _uow.SaveChangesAsync(ct);

        // 6. Issue tokens
        var roleCode = (await _uow.Repository<GlobalRole>().GetByIdAsync(user.RoleId ?? 0, ct))?.Code ?? GlobalRoleCodes.User;
        var fullName = $"{user.FirstName} {(user.MiddleName ?? "")} {user.LastName}".Trim();
        var access = _tokens.CreateToken(user.Id, user.EmailAddress, fullName, roleCode);
        var (plaintext, _, _) = await _refresh.IssueAsync(user.Id, null, null, ct);

        return PasskeyAuthResult.CreateSuccess(LoginResultDto.Issued(access, 1200, plaintext, 1200), "Signed in with passkey");
    }
}

/// <summary>
/// Result of passkey authentication/registration flow
/// </summary>
public class PasskeyAuthResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public object? Options { get; init; }
    public string Email { get; init; } = string.Empty;
    public bool IsNewUser { get; init; }
    public bool HasExistingPasskeys { get; init; }
    public LoginResultDto? LoginResult { get; init; }
    public string? Message { get; init; }

    public static PasskeyAuthResult OptionsReady(object options, string email, bool isNewUser, bool hasExistingPasskeys)
    {
        return new PasskeyAuthResult
        {
            Success = true,
            Options = options,
            Email = email,
            IsNewUser = isNewUser,
            HasExistingPasskeys = hasExistingPasskeys
        };
    }

    public static PasskeyAuthResult CreateSuccess(LoginResultDto loginResult, string message)
    {
        return new PasskeyAuthResult
        {
            Success = true,
            LoginResult = loginResult,
            Message = message
        };
    }

    public static PasskeyAuthResult Failed(string error)
    {
        return new PasskeyAuthResult
        {
            Success = false,
            Error = error
        };
    }
}
