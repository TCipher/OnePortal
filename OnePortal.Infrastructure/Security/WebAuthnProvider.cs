using System.Buffers.Text;
using System.Text.Json;
using System.Text;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.Extensions.Options;
using OnePortal.Application.Abstractions;
using OnePortal.Infrastructure.Security;

public class WebAuthnProvider : IWebAuthnProvider
{
    private readonly Fido2 _fido2;

    public WebAuthnProvider(IOptions<Fido2Options> opt)
    {
        var cfg = opt.Value;
        _fido2 = new Fido2(new Fido2Configuration
        {
            ServerDomain = cfg.RpId,
            ServerName = cfg.RpName,
            Origins = new HashSet<string>(cfg.Origins)
        });
    }

    public Task<string> BuildRegistrationOptionsAsync(
        int userId, string email, string displayName, IEnumerable<string> excludeCredentialIds, CancellationToken ct)
    {
        var user = new Fido2User
        {
            Id = BitConverter.GetBytes(userId),
            Name = email,
            DisplayName = displayName
        };

        var exclude = excludeCredentialIds
            .Select(id => new PublicKeyCredentialDescriptor(Convert.FromBase64String(id)))
            .ToList();

        // Enhanced options for GitHub-style registration with resident keys
        var options = _fido2.RequestNewCredential(new RequestNewCredentialParams
        {
            User = user,
            ExcludeCredentials = exclude,
            AuthenticatorSelection = new AuthenticatorSelection
            {
                ResidentKey = ResidentKeyRequirement.Preferred, // Enable resident keys for better UX
                UserVerification = UserVerificationRequirement.Required,
                // AuthenticatorAttachment = AuthenticatorAttachment.Any // Allow both platform and cross-platform
            },
            AttestationPreference = AttestationConveyancePreference.Direct, // Request direct attestation
            Extensions = new AuthenticationExtensionsClientInputs
            {
                // Enable credential properties for better user experience
                CredProps = true
            }
        });

        return Task.FromResult(options.ToJson()); // or JsonSerializer.Serialize(options)
    }

    public async Task<WebAuthnNewCredentialResult> VerifyAttestationAsync(
        string attestationResponseJson,
        string registrationOptionsJson,
        CancellationToken ct)
    {
        // FromJson() no longer exists on RawResponse => use System.Text.Json
        var att = JsonSerializer.Deserialize<AuthenticatorAttestationRawResponse>(attestationResponseJson)
                  ?? throw new InvalidOperationException("Invalid attestation payload");

        // CredentialCreateOptions.FromJson still exists, but this also works:
        var orig = JsonSerializer.Deserialize<CredentialCreateOptions>(registrationOptionsJson)
                   ?? throw new InvalidOperationException("Missing registration options");

        // NEW: use MakeNewCredentialParams (no StoredPublicKeyCredential needed)
        var result = await _fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
        {
            AttestationResponse = att,
            OriginalOptions = orig,
            IsCredentialIdUniqueToUserCallback = (args, _) => Task.FromResult(true) // DB uniqueness enforced separately
        }, ct);

        return new WebAuthnNewCredentialResult(
            CredentialId: Convert.ToBase64String(result.Id),
            PublicKey: result.PublicKey,
            SignCount: 0, // Counter not available in result, will be set during first authentication
            Aaguid: null, // Aaguid not available in result
            UserHandle: result.User.Id,
            AttestationFormat: result.AttestationFormat,
            Transports: Array.Empty<string>() // Transports not available in AuthenticatorAttestationRawResponse
        );
    }

    public Task<string> BuildAssertionOptionsAsync(
        IEnumerable<string> allowCredentialIds, CancellationToken ct)
    {
        Console.WriteLine($"[WebAuthnProvider] BuildAssertionOptionsAsync called with {allowCredentialIds.Count()} credential IDs");
        
        var allow = allowCredentialIds
            .Select(id => new PublicKeyCredentialDescriptor(Convert.FromBase64String(id)))
            .ToList();

        Console.WriteLine($"[WebAuthnProvider] Processed {allow.Count} allowed credentials");

        // Enhanced options for GitHub-style authentication with cross-device support
        var options = _fido2.GetAssertionOptions(new GetAssertionOptionsParams
        {
            AllowedCredentials = allow,
            UserVerification = UserVerificationRequirement.Required,
            Extensions = new AuthenticationExtensionsClientInputs()
        });

        Console.WriteLine($"[WebAuthnProvider] Got assertion options from Fido2: {options != null}");
        
        if (options == null)
        {
            Console.WriteLine("[WebAuthnProvider] Fido2 returned null options, creating fallback options");
            // Create enhanced fallback options for cross-device authentication
            var fallbackOptions = new
            {
                challenge = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)),
                timeout = 120000, // 2 minutes for cross-device
                rpId = "localhost", // TODO: Get from configuration
                allowCredentials = new object[0], // Empty to enable cross-device
                userVerification = "required",
                hints = new[] { "cross-platform" }, // Enable cross-device hints
                extensions = new
                {
                    appid = "https://localhost"
                }
            };
            
            var fallbackJson = System.Text.Json.JsonSerializer.Serialize(fallbackOptions);
            Console.WriteLine($"[WebAuthnProvider] Fallback options JSON: {fallbackJson}");
            return Task.FromResult(fallbackJson);
        }
        
        var json = options.ToJson();
        Console.WriteLine($"[WebAuthnProvider] Serialized options JSON length: {json?.Length ?? 0}");
        Console.WriteLine($"[WebAuthnProvider] Serialized options JSON: {json}");

        return Task.FromResult(json ?? string.Empty); // or JsonSerializer.Serialize(options)
    }

    public async Task<uint> VerifyAssertionAsync(
        string assertionResponseJson,
        string assertionOptionsJson,
        string credentialIdBase64Url,
        byte[] publicKey,
        uint signCount,
        byte[] userHandle,
        CancellationToken ct)
    {
        // FromJson() no longer exists on RawResponse => use System.Text.Json
        var assertion = JsonSerializer.Deserialize<AuthenticatorAssertionRawResponse>(assertionResponseJson)
                        ?? throw new InvalidOperationException("Invalid assertion payload");

        // AssertionOptions.FromJson also optional; keep consistent:
        var orig = JsonSerializer.Deserialize<AssertionOptions>(assertionOptionsJson)
                   ?? throw new InvalidOperationException("Missing assertion options");

        // NEW: MakeAssertionAsync with MakeAssertionParams (StoredPublicKeyCredential removed)
        var res = await _fido2.MakeAssertionAsync(new MakeAssertionParams
        {
            AssertionResponse = assertion,
            OriginalOptions = orig,
            StoredPublicKey = publicKey,
            StoredSignatureCounter = signCount,
            IsUserHandleOwnerOfCredentialIdCallback = (args, _) =>
            {
                // Validate the credential id & user handle match what we stored
                var credIdOk = Convert.ToBase64String(args.CredentialId) == credentialIdBase64Url;
                var handleOk = userHandle is null || userHandle.Length == 0 ||
                               (args.UserHandle != null && args.UserHandle.SequenceEqual(userHandle));
                return Task.FromResult(credIdOk && handleOk);
            }
        }, ct);

        return res.SignCount; // persist this as new SignCount
    }

    public Task<string> BuildUniversalPasskeyOptionsAsync(
        string email,
        string displayName,
        IEnumerable<string> existingCredentialIds,
        CancellationToken ct)
    {
        Console.WriteLine($"[WebAuthnProvider] Building universal options for: {email}");
        
        // Create user object for registration
        var user = new Fido2User
        {
            Id = System.Text.Encoding.UTF8.GetBytes(email), // Use email as user ID
            Name = email,
            DisplayName = displayName
        };

        var existingCreds = existingCredentialIds.ToList();
        Console.WriteLine($"[WebAuthnProvider] Found {existingCreds.Count} existing credentials");

        // Build options that work for both authentication and registration
        var universalOptions = new
        {
            // Challenge
            challenge = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)),
            
            // Relying Party
            rp = new
            {
                name = "OnePortal", // TODO: Get from configuration
                id = "localhost" // TODO: Get from configuration
            },
            
            // User info (for registration)
            user = new
            {
                id = Convert.ToBase64String(user.Id),
                name = user.Name,
                displayName = user.DisplayName
            },
            
            // Allow credentials for authentication
            allowCredentials = existingCreds.Select(id => new
            {
                type = "public-key",
                id = id,
                transports = new[] { "internal", "hybrid" }
            }).ToArray(),
            
            // Public key parameters
            pubKeyCredParams = new[]
            {
                new { type = "public-key", alg = -7 },   // ES256
                new { type = "public-key", alg = -257 }  // RS256
            },
            
            // Authenticator selection for registration
            authenticatorSelection = new
            {
                authenticatorAttachment = "any", // Allow both platform and cross-platform
                userVerification = "required",
                requireResidentKey = true, // Enable resident keys for better UX
                residentKey = "preferred"
            },
            
            // Attestation preference
            attestation = "direct",
            
            // Timeout for cross-device
            timeout = 120000, // 2 minutes
            
            // Extensions for cross-device support
            extensions = new
            {
                appid = "https://localhost",
                credProps = true
            },
            
            // Hints for better user experience
            hints = new[] { "security-key", "client-device", "cross-platform" }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(universalOptions, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        Console.WriteLine($"[WebAuthnProvider] Generated universal options JSON length: {json.Length}");
        return Task.FromResult(json);
    }
}
