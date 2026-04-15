using CleanArch26.Application.Interfaces;
using CleanArch26.Domain.Entities;

namespace CleanArch26.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of IProductRepository.
/// In a real project this would use EF Core / a database.
/// </summary>
public sealed class ProductRepository : IProductRepository
{
    // Seed data – mimics what a real DB would contain
    private static readonly IReadOnlyList<Product> _products =
    [
        new Product
        {
            Id = Guid.Parse("11111111-0000-0000-0000-000000000001"),
            Name = "Laptop Pro 15",
            Description = "High-performance laptop with 32 GB RAM and 1 TB SSD.",
            Price = 1_499.99m,
            Category = "Electronics"
        },
        new Product
        {
            Id = Guid.Parse("11111111-0000-0000-0000-000000000002"),
            Name = "Wireless Keyboard",
            Description = "Compact mechanical keyboard with Bluetooth 5.0.",
            Price = 89.95m,
            Category = "Accessories"
        },
        new Product
        {
            Id = Guid.Parse("11111111-0000-0000-0000-000000000003"),
            Name = "4K Monitor 27\"",
            Description = "Ultra-wide 4K display with 144 Hz refresh rate.",
            Price = 699.00m,
            Category = "Electronics"
        }
    ];

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(product);
    }

    public Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Product>>(_products);
    }
}
