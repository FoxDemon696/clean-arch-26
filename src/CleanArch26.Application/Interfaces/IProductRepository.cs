using CleanArch26.Domain.Entities;

namespace CleanArch26.Application.Interfaces;

/// <summary>
/// Repository interface for Product persistence (defined in Application, implemented in Infrastructure).
/// </summary>
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default);
}
