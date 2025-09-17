using System.Buffers.Text;
using System.Text.Json;
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

        // NEW: use RequestNewCredentialParams
        var options = _fido2.RequestNewCredential(new RequestNewCredentialParams
        {
            User = user,
            ExcludeCredentials = exclude,
            AuthenticatorSelection = new AuthenticatorSelection
            {
                ResidentKey = ResidentKeyRequirement.Discouraged,
                UserVerification = UserVerificationRequirement.Required
            },
            AttestationPreference = AttestationConveyancePreference.None
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
        var allow = allowCredentialIds
            .Select(id => new PublicKeyCredentialDescriptor(Convert.FromBase64String(id)))
            .ToList();

        // NEW: use GetAssertionOptionsParams
        var options = _fido2.GetAssertionOptions(new GetAssertionOptionsParams
        {
            AllowedCredentials = allow,
            UserVerification = UserVerificationRequirement.Required
        });

        return Task.FromResult(options.ToJson()); // or JsonSerializer.Serialize(options)
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
}
