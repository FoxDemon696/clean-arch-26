using CleanArch26.Application.Interfaces;
using CleanArch26.Infrastructure.ExternalServices;
using CleanArch26.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArch26.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Repository
        services.AddScoped<IProductRepository, ProductRepository>();

        // External inventory service – registered as a typed HttpClient
        services.AddHttpClient<IExternalInventoryService, ExternalInventoryServiceClient>(client =>
        {
            // Replace with the real base address from configuration in production
            client.BaseAddress = new Uri("https://inventory.example.com/api/");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        return services;
    }
}
