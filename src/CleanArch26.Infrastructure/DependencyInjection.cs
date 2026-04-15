using CleanArch26.Application.Interfaces;
using CleanArch26.Infrastructure.ExternalServices;
using CleanArch26.Infrastructure.Persistence;
using CleanArch26.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArch26.Infrastructure;

public static class DependencyInjection
{
    /// <param name="connectionString">
    /// Passed in from the Api layer (read from configuration) so Infrastructure
    /// never has to reference IConfiguration directly.
    /// </param>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        string connectionString)
    {
        // EF Core – SQLite for development; swap UseSqlite for UseSqlServer in production
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        // Repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        // External inventory service – typed HttpClient
        services.AddHttpClient<IExternalInventoryService, ExternalInventoryServiceClient>(client =>
        {
            // Replace with the real base address from configuration in production
            client.BaseAddress = new Uri("https://inventory.example.com/api/");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        return services;
    }
}
