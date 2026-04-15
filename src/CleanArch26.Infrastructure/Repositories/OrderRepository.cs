using CleanArch26.Application.Interfaces;
using CleanArch26.Domain.Entities;
using CleanArch26.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CleanArch26.Infrastructure.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
        => _context = context;

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .ToListAsync(cancellationToken);
}
