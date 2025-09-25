using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OnePortal.Blazor.Services.Auth
{
    public class JwtAuthStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal _user = new(new ClaimsIdentity());
        private readonly LocalStorageService _storage;
        public ClaimsPrincipal CurrentUser => _user;

        public JwtAuthStateProvider(LocalStorageService storage)
        {
            _storage = storage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Check if we already have a user set
            if (_user.Identity?.IsAuthenticated == true)
            {
                return new AuthenticationState(_user);
            }

            // Try to get token from localStorage
            try
            {
                var token = await _storage.GetItemAsync("onep.access");
                if (!string.IsNullOrWhiteSpace(token))
                {
                    // Validate and parse the token
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);
                    
                    // Check if token is expired
                    if (jwtToken.ValidTo > DateTime.UtcNow)
                    {
                        var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
                        _user = new ClaimsPrincipal(identity);
                        return new AuthenticationState(_user);
                    }
                    else
                    {
                        // Token is expired, remove it
                        await _storage.RemoveItemAsync("onep.access");
                    }
                }
            }
            catch (Exception ex)
            {
                // If there's an error reading the token, remove it
                Console.WriteLine($"Error reading token from storage: {ex.Message}");
                await _storage.RemoveItemAsync("onep.access");
            }

            // Return unauthenticated state
            _user = new ClaimsPrincipal(new ClaimsIdentity());
            return new AuthenticationState(_user);
        }

        public Task NotifyUserAuthentication(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            var identity = new ClaimsIdentity(token.Claims, "jwt");
            _user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return Task.CompletedTask;
        }

        public Task NotifyUserLogout()
        {
            _user = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return Task.CompletedTask;
        }
    }
}
