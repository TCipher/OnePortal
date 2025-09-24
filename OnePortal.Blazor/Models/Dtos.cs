namespace OnePortal.Blazor.Models
{
    public record LoginRequest(string Email, string Password);
    public record LoginResponse(string? TokenType, string? AccessToken, int? ExpiresIn, string? RefreshToken, int? RefreshExpiresIn, MfaInfo? Mfa, bool MustChangePassword);
    public record MfaInfo(bool Required, string? Method, string? ChallengeId);

    // API Response wrapper
    public record ApiResponse<T>(bool Ok, T? Data, ApiError? Error = null);
    public record ApiError(string Code, string Message, object? Details = null);


    public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
    public record ApiResult(bool Ok, string? Error);


    public record SendOtpRequest(string Email, string? ChallengeId = null);
    public record VerifyOtpRequest(string Email, string Otp, string? ChallengeId);


    public record ForgotPasswordRequest(string Email);
    public record ResetPasswordRequest(string Token, string NewPassword);


    public record WebAuthnStartRequest(string Email);
    public record WebAuthnStartResponse(object PublicKeyOptions);
    public record WebAuthnFinishRequest(string Email, object Credential);
    public record WebAuthnFinishResponse(LoginResponse? LoginResult);
    public record WebAuthnRegistrationRequest(string DisplayName);

    public record WebAuthnResult(bool Success, string? Error, bool NoPasskeysRegistered = false)
    {
        public static WebAuthnResult CreateSuccess() => new(true, null, false);
        public static WebAuthnResult CreateFailed(string error) => new(false, error, false);
        public static WebAuthnResult CreateNoPasskeysRegistered() => new(false, "No passkeys registered", true);
    }
}
