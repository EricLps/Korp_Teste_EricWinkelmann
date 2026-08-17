using System.Net.Http.Json;
using System.Text.Json;

namespace BillingService.Services;

public class StockHttpClient
{
    private readonly HttpClient _http;

    public StockHttpClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<HttpResponseMessage> ReserveAsync(Guid productId, object payload)
    {
        var response = await _http.PostAsJsonAsync($"api/products/{productId}/reserve", payload);
        return response;
    }

    public async Task<HttpResponseMessage> ConfirmReservationAsync(Guid productId, object payload)
    {
        var response = await _http.PostAsJsonAsync($"api/products/{productId}/confirm-reservation", payload);
        return response;
    }

    public async Task<HttpResponseMessage> CancelReservationAsync(Guid productId, object payload)
    {
        var response = await _http.PostAsJsonAsync($"api/products/{productId}/cancel-reservation", payload);
        return response;
    }

    public async Task<T?> GetAsync<T>(string path)
    {
        var response = await _http.GetAsync(path);
        if (!response.IsSuccessStatusCode) return default;
        return await response.Content.ReadFromJsonAsync<T>();
    }
}
