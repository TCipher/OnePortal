using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;
using OnePortal.Blazor.Services.Auth;

namespace OnePortal.Blazor.Services.Http
{
    public sealed class AuthMessageHandler : DelegatingHandler
    {
            private readonly LocalStorageService _storage;
            private readonly NavigationManager _nav;

            // Keep these in sync with AuthService
            private const string AccessKey = "onep.access";
            private const string RefreshKey = "onep.refresh";
            private const string EmailKey = "onep.email";
            private const string MustChangeKey = "onep.mustchange";

            public AuthMessageHandler(LocalStorageService storage, NavigationManager nav)
            {
                _storage = storage;
                _nav = nav;
            }

            protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var token = await _storage.GetItemAsync(AccessKey);
                if (!string.IsNullOrWhiteSpace(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var res = await base.SendAsync(request, cancellationToken);

                if (res.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // local, no-HTTP logout
                    await _storage.RemoveItemAsync(AccessKey);
                    await _storage.RemoveItemAsync(RefreshKey);
                    await _storage.RemoveItemAsync(EmailKey);
                    await _storage.RemoveItemAsync(MustChangeKey);

                    _nav.NavigateTo("/login?expired=1", forceLoad: true);
                }

                return res;
            }
        }
    }
