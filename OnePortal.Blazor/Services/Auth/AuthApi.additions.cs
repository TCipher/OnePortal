using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using OnePortal.Blazor.Models;
using OnePortal.Blazor.Services.Http;

namespace OnePortal.Blazor.Services.Auth
{
    public partial class AuthApi
    {
        public Task<List<PortalAccessItem>?> GetPortalAccessForUserAsync(int userId, CancellationToken ct = default)
            => _api.GetApiResponseAsync<List<PortalAccessItem>>($"api/portalaccess/user/{userId}", ct);

        public async Task<List<PortalNavItem>> GetMyPortalsAsync(ClaimsPrincipal user, CancellationToken ct = default)
        {
            var id = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user?.FindFirst("sub")?.Value;
            if (!int.TryParse(id, out var userId)) return GetDefaultPortals();

            try
            {
                var access = await GetPortalAccessForUserAsync(userId, ct) ?? new List<PortalAccessItem>();

                // Map the accessible portals to navigation items.  Only active
                // assignments are considered.  We build a list of distinct
                // PortalNavItem entries so that if a portal appears multiple times
                // in the access list it only shows once in the sidebar.
                var userPortals = access
                    .Where(a => a.IsActive)
                    .Select(a => new PortalNavItem(
                        Code: a.PortalCode,
                        Name: a.PortalName,
                        Url: $"/{a.PortalCode.ToLower()}",
                        IconSvg: GetPortalIcon(a.PortalCode)))
                    .GroupBy(p => p.Code, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToList();

                // Ensure the SIMS portal is always available for logged in users.
                // If the user already has SIMS access, we don't duplicate it.
                var simsCode = "SIMS";
                if (userPortals.All(p => !p.Code.Equals(simsCode, StringComparison.OrdinalIgnoreCase)))
                {
                    userPortals.Insert(0, new PortalNavItem(
                        Code: simsCode,
                        Name: "SIMS",
                        Url: "/sims",
                        IconSvg: GetPortalIcon(simsCode)));
                }

                // If no portals were found at all (unlikely if SIMS is appended)
                // then fall back to a simple default list for demonstration.
                return userPortals.Any() ? userPortals : GetDefaultPortals();
            }
            catch
            {
                // If API call fails, return default portals for demo
                return GetDefaultPortals();
            }
        }

        private static List<PortalNavItem> GetDefaultPortals()
        {
            return new List<PortalNavItem>
            {
                new("MEDICAL", "Medical", "/medical", GetPortalIcon("MEDICAL")),
                new("ESERVICE", "E-Service", "/eservice", GetPortalIcon("ESERVICE")),
                new("REGULATORY", "Regulatory", "/regulatory", GetPortalIcon("REGULATORY"))
            };
        }

        private static string? GetPortalIcon(string portalCode)
        {
            return portalCode.ToUpper() switch
            {
                "MEDICAL" => @"<svg width=""20"" height=""20"" viewBox=""0 0 20 20"" fill=""none"">
                    <path d=""M10 2L3 7V18H17V7L10 2Z"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""/>
                    <path d=""M10 7V12"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""/>
                    <path d=""M7 9.5H13"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""/>
                </svg>",
                "ESERVICE" => @"<svg width=""20"" height=""20"" viewBox=""0 0 20 20"" fill=""none"">
                    <rect x=""2"" y=""3"" width=""16"" height=""14"" rx=""2"" stroke=""currentColor"" stroke-width=""2""/>
                    <path d=""M8 21L12 17L8 13"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""/>
                </svg>",
                "REGULATORY" => @"<svg width=""20"" height=""20"" viewBox=""0 0 20 20"" fill=""none"">
                    <path d=""M9 12L11 14L15 10"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""/>
                    <path d=""M21 12C21 16.9706 16.9706 21 12 21C7.02944 21 3 16.9706 3 12C3 7.02944 7.02944 3 12 3C16.9706 3 21 7.02944 21 12Z"" stroke=""currentColor"" stroke-width=""2""/>
                </svg>",
                "SIMS" => @"<svg width=""20"" height=""20"" viewBox=""0 0 20 20"" fill=""none"">
                    <path d=""M4 4H16V16H4V4Z"" stroke=""currentColor"" stroke-width=""2"" stroke-linejoin=""round""/>
                    <path d=""M4 10H16"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round""/>
                    <path d=""M10 4V16"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round""/>
                </svg>",
                _ => null
            };
        }
    }
}
