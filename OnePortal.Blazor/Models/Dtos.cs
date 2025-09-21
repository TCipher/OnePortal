namespace OnePortal.Blazor.Models
{
    public record LoginRequest(string Email, string Password);
    public record LoginResponse(string? AccessToken, string? RefreshToken, bool MustChangePassword, MfaInfo? Mfa);
    public record MfaInfo(bool Required, string? Method, string? ChallengeId);


    public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
    public record ApiResult(bool Ok, string? Error);


    public record SendOtpRequest(string Email);
    public record VerifyOtpRequest(string ChallengeId, string Code);


    public record ForgotPasswordRequest(string Email);
    public record ResetPasswordRequest(string Token, string NewPassword);


    public record WebAuthnStartRequest(string Email);
    public record WebAuthnStartResponse(string Challenge, object PublicKeyOptions);
    public record WebAuthnFinishRequest(string Email, object Credential);
    public record WebAuthnFinishResponse(bool Ok, string? Error);
}
