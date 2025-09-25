using OnePortal.Blazor.Models;
using OnePortal.Blazor.Services.Http;
using static System.Net.WebRequestMethods;

namespace OnePortal.Blazor.Services.Auth
{
    public partial class AuthApi
    {
        private readonly ApiClient _api;
        public AuthApi(HttpClient http) => _api = new ApiClient(http);


        public Task<LoginResponse?> LoginAsync(LoginRequest r, CancellationToken ct = default)
        => _api.PostApiResponseAsync<LoginResponse>("api/auth/login", r, ct);


        public Task<ApiResult?> ChangePasswordAsync(ChangePasswordRequest r, CancellationToken ct = default)
        => _api.PostApiResponseAsync<ApiResult>("api/account/change-password", r, ct);


        public Task<ApiResult?> SendEmailOtpAsync(SendOtpRequest r, CancellationToken ct = default)
        => _api.PostApiResponseAsync<ApiResult>("api/auth/otp/send", r, ct);


        public Task<LoginResponse?> VerifyEmailOtpAsync(VerifyOtpRequest r, CancellationToken ct = default)
        => _api.PostApiResponseAsync<LoginResponse>("api/auth/otp/verify", r, ct);


        public Task<WebAuthnStartResponse?> WebAuthnStartAsync(WebAuthnStartRequest r, CancellationToken ct = default)
        => _api.PostApiResponseAsync<WebAuthnStartResponse>("api/registration/begin", r, ct);


        public Task<WebAuthnFinishResponse?> WebAuthnFinishAsync(WebAuthnFinishRequest r, CancellationToken ct = default)
        => _api.PostApiResponseAsync<WebAuthnFinishResponse>("api/webauthn/registration/finish", r, ct);

        // WebAuthn Registration endpoints
        public Task<WebAuthnStartResponse?> WebAuthnBeginRegistrationAsync(WebAuthnRegistrationRequest r, CancellationToken ct = default)
        => _api.PostApiResponseAsync<WebAuthnStartResponse>("api/auth/webauthn/registration/begin", r, ct);

        public Task<ApiResult?> WebAuthnFinishRegistrationAsync(WebAuthnFinishRequest r, CancellationToken ct = default)
        => _api.PostApiResponseAsync<ApiResult>("api/auth/webauthn/registration/finish", r, ct);

        // WebAuthn Assertion (Login) endpoints
        public Task<WebAuthnStartResponse?> WebAuthnBeginAssertionAsync(WebAuthnStartRequest r, CancellationToken ct = default)
        => _api.PostApiResponseAsync<WebAuthnStartResponse>("api/auth/webauthn/assertion/begin", r, ct);


        public Task<LoginResponse?> WebAuthnFinishAssertionAsync(WebAuthnFinishRequest r, CancellationToken ct = default)
        => _api.PostApiResponseAsync<LoginResponse>("api/auth/webauthn/assertion/finish", r, ct);


        public Task<ApiResult?> ForgotPasswordAsync(ForgotPasswordRequest r, CancellationToken ct = default)
        => _api.PostApiResponseAsync<ApiResult>("api/auth/forgot-password", r, ct);


        public Task<ApiResult?> ResetPasswordAsync(ResetPasswordRequest r, CancellationToken ct = default)
        => _api.PostApiResponseAsync<ApiResult>("api/auth/reset-password", r, ct);
    }

}
