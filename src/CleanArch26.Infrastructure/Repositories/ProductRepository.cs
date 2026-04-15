using CleanArch26.Application.Interfaces;
using CleanArch26.Domain.Entities;
using CleanArch26.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CleanArch26.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
        => _context = context;

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Products
            .Include(p => p.Category)
            .ToListAsync(cancellationToken);
}
