using CleanArch26.Application.Interfaces;
using CleanArch26.Domain.Entities;
using CleanArch26.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CleanArch26.Infrastructure.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
        => _context = context;

    public async Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Categories
            .Include(c => c.Products)
            .ToListAsync(cancellationToken);

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
}
