using OnePortal.Blazor.Models;
using System.Net.Http.Json;

namespace OnePortal.Blazor.Services.Http
{
    public partial class ApiClient
    {
        private readonly HttpClient _http;
        public ApiClient(HttpClient http) => _http = http;


        public async Task<T?> GetAsync<T>(string url, CancellationToken ct = default) =>
        await _http.GetFromJsonAsync<T>(url, cancellationToken: ct);


        public async Task<T?> PostAsync<T>(string url, object body, CancellationToken ct = default)
        {
            var res = await _http.PostAsJsonAsync(url, body, ct);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
        }

        // Special method for API responses that are wrapped in ApiResponse<T>
        public async Task<T?> PostApiResponseAsync<T>(string url, object body, CancellationToken ct = default)
        {
            var res = await _http.PostAsJsonAsync(url, body, ct);
            res.EnsureSuccessStatusCode();
            var apiResponse = await res.Content.ReadFromJsonAsync<ApiResponse<T>>(cancellationToken: ct);
            return apiResponse?.Ok == true ? apiResponse.Data : default;
        }
    }
}
