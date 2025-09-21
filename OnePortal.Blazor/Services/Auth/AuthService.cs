using Microsoft.AspNetCore.Components;
using OnePortal.Blazor.Models;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Components.Authorization;


namespace OnePortal.Blazor.Services.Auth
{
    public class AuthService
    {
        private readonly AuthApi _api;
        private readonly JwtAuthStateProvider _state;
        private readonly NavigationManager _nav;
        private readonly LocalStorageService _storage;


        private const string AccessKey = "onep.access";
        private const string RefreshKey = "onep.refresh";
        private const string EmailKey = "onep.email";
        private const string MustChangeKey = "onep.mustchange";

        public async Task<bool> HasTempTokenAsync()
        {
            var token = await GetLocal(AccessKey);
            return !string.IsNullOrWhiteSpace(token);
        }



        public AuthService(AuthApi api, JwtAuthStateProvider state, NavigationManager nav, LocalStorageService storage)
        { _api = api; _state = state; _nav = nav; _storage = storage; }


        public async Task<(bool ok, string? err)> LoginAsync(string email, string password)
        {
            try
            {
                var res = await _api.LoginAsync(new LoginRequest(email, password));
                if (res is null || string.IsNullOrWhiteSpace(res.AccessToken))
                    return (false, "Invalid credentials");


               await  SetLocal(AccessKey, res.AccessToken!);
               await  SetLocal(EmailKey, email);
               await  SetLocal(MustChangeKey, res.MustChangePassword ? "1" : "0");
                await _state.NotifyUserAuthentication(res.AccessToken!);


                if (res.MustChangePassword) _nav.NavigateTo("/change-password", true);
                else _nav.NavigateTo("/mfa/choice", true);
                return (true, null);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return (false, "Login failed");
            }
        }
        public async Task ForceReloginAsync()
        {
            await LogoutAsync();
            _nav.NavigateTo("/login", true);
        }


        public async Task LogoutAsync()
        {
            await RemoveLocal(AccessKey);
            await RemoveLocal(RefreshKey); 
            await RemoveLocal(MustChangeKey);
            await _state.NotifyUserLogout();
        }


        public async Task<string?> GetAccessTokenAsync() =>
    await GetLocal(AccessKey);



        public ClaimsPrincipal GetPrincipal() => _state.CurrentUser ?? new ClaimsPrincipal(new ClaimsIdentity());


        private async Task<string?> GetLocal(string key) =>
     await _storage.GetItemAsync(key);

        private async Task SetLocal(string key, string value) =>
            await _storage.SetItemAsync(key, value);

        private async Task RemoveLocal(string key) =>
            await _storage.RemoveItemAsync(key);
    }
}
