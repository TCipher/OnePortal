using OnePortal.Blazor.Models;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OnePortal.Blazor.Services.Http
{
    public partial class ApiClient
    {
        public async Task<T?> GetApiResponseAsync<T>(string url, CancellationToken ct = default)
        {
            var apiResponse = await _http.GetFromJsonAsync<ApiResponse<T>>(url, cancellationToken: ct);
            return apiResponse?.Ok == true ? apiResponse.Data : default;
        }
    }
}
