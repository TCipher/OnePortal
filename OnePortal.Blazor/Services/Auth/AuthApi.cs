using OnePortal.Blazor.Models;
using OnePortal.Blazor.Services.Http;

namespace OnePortal.Blazor.Services.Auth
{
    public class AuthApi
    {
        private readonly ApiClient _api;
        public AuthApi(HttpClient http) => _api = new ApiClient(http);


        public Task<LoginResponse?> LoginAsync(LoginRequest r, CancellationToken ct = default)
        => _api.PostAsync<LoginResponse>("api/auth/login", r, ct);


        public Task<ApiResult?> ChangePasswordAsync(ChangePasswordRequest r, CancellationToken ct = default)
        => _api.PostAsync<ApiResult>("api/auth/change-password", r, ct);


        public Task<ApiResult?> SendEmailOtpAsync(SendOtpRequest r, CancellationToken ct = default)
        => _api.PostAsync<ApiResult>("api/auth/otp/send", r, ct);


        public Task<ApiResult?> VerifyEmailOtpAsync(VerifyOtpRequest r, CancellationToken ct = default)
        => _api.PostAsync<ApiResult>("api/auth/otp/verify", r, ct);


        public Task<WebAuthnStartResponse?> WebAuthnStartAsync(WebAuthnStartRequest r, CancellationToken ct = default)
        => _api.PostAsync<WebAuthnStartResponse>("api/registration/begin", r, ct);


        public Task<WebAuthnFinishResponse?> WebAuthnFinishAsync(WebAuthnFinishRequest r, CancellationToken ct = default)
        => _api.PostAsync<WebAuthnFinishResponse>("api/webauthn/registration/finish", r, ct);


        public Task<ApiResult?> ForgotPasswordAsync(ForgotPasswordRequest r, CancellationToken ct = default)
        => _api.PostAsync<ApiResult>("api/auth/forgot-password", r, ct);


        public Task<ApiResult?> ResetPasswordAsync(ResetPasswordRequest r, CancellationToken ct = default)
        => _api.PostAsync<ApiResult>("api/auth/reset-password", r, ct);
    }
}
