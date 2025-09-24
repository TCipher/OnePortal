using OnePortal.Blazor.Models;

namespace OnePortal.Blazor.Services.Auth
{
    public class OtpService(AuthApi api)
    {
        private readonly AuthApi _api = api;


        public Task<ApiResult?> SendAsync(string email, string? challengeId = null) => _api.SendEmailOtpAsync(new SendOtpRequest(email, challengeId));
        public Task<LoginResponse?> VerifyAsync(string email, string challengeId, string code) => _api.VerifyEmailOtpAsync(new VerifyOtpRequest(email, code, challengeId));
    }
}
