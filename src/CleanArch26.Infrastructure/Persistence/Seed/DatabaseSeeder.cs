using CleanArch26.Domain.Entities;
using CleanArch26.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CleanArch26.Infrastructure.Persistence.Seed;

/// <summary>
/// Seeds the database with initial reference data.
/// Called once on startup; skips silently if data is already present.
/// For production, consider moving seed data into EF Core migrations instead.
/// </summary>
public static class DatabaseSeeder
{
    // Stable GUIDs so re-seeding is idempotent
    private static readonly Guid ElectronicsId = Guid.Parse("22222222-0000-0000-0000-000000000001");
    private static readonly Guid AccessoriesId  = Guid.Parse("22222222-0000-0000-0000-000000000002");

    private static readonly Guid LaptopId    = Guid.Parse("11111111-0000-0000-0000-000000000001");
    private static readonly Guid KeyboardId  = Guid.Parse("11111111-0000-0000-0000-000000000002");
    private static readonly Guid MonitorId   = Guid.Parse("11111111-0000-0000-0000-000000000003");

    private static readonly Guid OrderId     = Guid.Parse("33333333-0000-0000-0000-000000000001");

    public static async Task SeedAsync(AppDbContext context)
    {
        // Guard: skip if already seeded
        if (await context.Categories.AnyAsync())
            return;

        var categories = new[]
        {
            new Category { Id = ElectronicsId, Name = "Electronics",  Description = "Electronic devices and components" },
            new Category { Id = AccessoriesId, Name = "Accessories",  Description = "Peripherals and accessories" }
        };

        var products = new[]
        {
            new Product { Id = LaptopId,   Name = "Laptop Pro 15",    Description = "High-performance laptop with 32 GB RAM and 1 TB SSD.", Price = 1_499.99m, CategoryId = ElectronicsId },
            new Product { Id = KeyboardId, Name = "Wireless Keyboard", Description = "Compact mechanical keyboard with Bluetooth 5.0.",    Price = 89.95m,   CategoryId = AccessoriesId },
            new Product { Id = MonitorId,  Name = "4K Monitor 27\"",   Description = "Ultra-wide 4K display with 144 Hz refresh rate.",    Price = 699.00m,  CategoryId = ElectronicsId }
        };

        var order = new Order
        {
            Id            = OrderId,
            CustomerName  = "Jane Doe",
            CustomerEmail = "jane.doe@example.com",
            OrderDate     = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc),
            Status        = OrderStatus.Confirmed,
            Items         =
            [
                new OrderItem { Id = Guid.Parse("44444444-0000-0000-0000-000000000001"), OrderId = OrderId, ProductId = LaptopId,   Quantity = 1, UnitPrice = 1_499.99m },
                new OrderItem { Id = Guid.Parse("44444444-0000-0000-0000-000000000002"), OrderId = OrderId, ProductId = KeyboardId, Quantity = 2, UnitPrice = 89.95m    }
            ]
        };

        await context.Categories.AddRangeAsync(categories);
        await context.Products.AddRangeAsync(products);
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();
    }
}
