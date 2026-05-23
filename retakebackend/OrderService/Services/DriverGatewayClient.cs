using System.Net.Http.Json;
using OrderService.Contracts;

namespace OrderService.Services;

public class DriverGatewayClient(HttpClient httpClient, IConfiguration configuration)
{
    private readonly string _baseUrl = configuration["DriverService:BaseUrl"] ?? "http://localhost:5002";

    public async Task<DriverSummary?> GetAvailableDriverAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<DriverSummary>($"{_baseUrl}/drivers/available", cancellationToken);
    }

    public async Task<bool> AssignOrderToDriverAsync(Guid driverId, Guid orderId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync($"{_baseUrl}/drivers/{driverId}/assign-order", new AssignOrderToDriverRequest(orderId), cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
