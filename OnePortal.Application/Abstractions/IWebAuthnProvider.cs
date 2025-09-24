namespace OnePortal.Application.Abstractions;

public record WebAuthnNewCredentialResult(
    string CredentialId,
    byte[] PublicKey,
    uint SignCount,
    string? Aaguid,
    byte[] UserHandle,
    string? AttestationFormat,
    string[] Transports
);

public interface IWebAuthnProvider
{
    // Build JSON for navigator.credentials.create
    Task<string> BuildRegistrationOptionsAsync(
        int userId,
        string email,
        string displayName,
        IEnumerable<string> excludeCredentialIds,
        CancellationToken ct);

    // Verify attestation & return new credential details
    Task<WebAuthnNewCredentialResult> VerifyAttestationAsync(
        string attestationResponseJson,
        string registrationOptionsJson,
        CancellationToken ct);

    // Build JSON for navigator.credentials.get
    Task<string> BuildAssertionOptionsAsync(
        IEnumerable<string> allowCredentialIds,
        CancellationToken ct);

    // Verify assertion & return updated counter
    Task<uint> VerifyAssertionAsync(
        string assertionResponseJson,
        string assertionOptionsJson,
        string credentialIdBase64Url,
        byte[] publicKey,
        uint signCount,
        byte[] userHandle,
        CancellationToken ct);

    // Build universal options that work for both authentication and registration (GitHub-style)
    Task<string> BuildUniversalPasskeyOptionsAsync(
        string email,
        string displayName,
        IEnumerable<string> existingCredentialIds,
        CancellationToken ct);
}
