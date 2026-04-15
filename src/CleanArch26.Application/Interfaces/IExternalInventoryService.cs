namespace CleanArch26.Application.Interfaces;

/// <summary>
/// Interface for the external inventory service (defined in Application, implemented in Infrastructure).
/// Infrastructure contains the actual HTTP client / mock; Application only knows this contract.
/// </summary>
public interface IExternalInventoryService
{
    /// <summary>Returns the available stock count for a product from the external inventory system.</summary>
    Task<int> GetStockLevelAsync(Guid productId, CancellationToken cancellationToken = default);
}
