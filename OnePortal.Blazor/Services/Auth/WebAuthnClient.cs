using Microsoft.JSInterop;
using OnePortal.Blazor.Models;

namespace OnePortal.Blazor.Services.Auth
{
    public class WebAuthnClient(IJSRuntime js, AuthApi api)
    {
        private readonly IJSRuntime _js = js;
        private readonly AuthApi _api = api;


        public async Task<bool> AuthenticateAsync(string email)
        {
            var start = await _api.WebAuthnStartAsync(new WebAuthnStartRequest(email));
            if (start is null) return false;
            var credential = await _js.InvokeAsync<object>("webauthn.get", start.PublicKeyOptions);
            var finish = await _api.WebAuthnFinishAsync(new WebAuthnFinishRequest(email, credential));
            return finish?.Ok == true;
        }
    }
}
