using System.Net.Http.Json;
using CleanArch26.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CleanArch26.Infrastructure.ExternalServices;

/// <summary>
/// HTTP client adapter for the external inventory system.
/// This is the correct place for external integrations: Infrastructure keeps
/// all third-party concerns away from Application and Domain.
///
/// The mock response simulates what the real endpoint would return so the
/// rest of the system can be developed and tested without a live dependency.
/// </summary>
public sealed class ExternalInventoryServiceClient : IExternalInventoryService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalInventoryServiceClient> _logger;

    // Seeded stock levels keyed by product id – used as fallback / mock
    private static readonly Dictionary<Guid, int> _mockStock = new()
    {
        [Guid.Parse("11111111-0000-0000-0000-000000000001")] = 12,
        [Guid.Parse("11111111-0000-0000-0000-000000000002")] = 47,
        [Guid.Parse("11111111-0000-0000-0000-000000000003")] = 5
    };

    public ExternalInventoryServiceClient(
        HttpClient httpClient,
        ILogger<ExternalInventoryServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<int> GetStockLevelAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Real call would be something like:
            //   var response = await _httpClient.GetFromJsonAsync<InventoryResponse>(
            //       $"inventory/{productId}", cancellationToken);
            //   return response?.StockLevel ?? 0;

            // --- Mock: simulate a short network round-trip ---
            await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken);

            return _mockStock.TryGetValue(productId, out var level) ? level : 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to retrieve stock level for product {ProductId} from external inventory service. Returning 0.",
                productId);
            return 0;
        }
    }

    // Private DTO that mirrors the shape the real API would return
    private sealed record InventoryResponse(Guid ProductId, int StockLevel);
}
