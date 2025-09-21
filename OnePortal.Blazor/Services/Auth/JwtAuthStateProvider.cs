using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OnePortal.Blazor.Services.Auth
{
    public class JwtAuthStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal _user = new(new ClaimsIdentity());
        public ClaimsPrincipal CurrentUser => _user;


        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(_user));


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
