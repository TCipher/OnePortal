using OnePortal.Blazor.Models;

namespace OnePortal.Blazor.Services.Auth
{
    public class OtpService(AuthApi api)
    {
        private readonly AuthApi _api = api;


        public Task<ApiResult?> SendAsync(string email) => _api.SendEmailOtpAsync(new SendOtpRequest(email));
        public Task<ApiResult?> VerifyAsync(string challengeId, string code) => _api.VerifyEmailOtpAsync(new VerifyOtpRequest(challengeId, code));
    }
}
