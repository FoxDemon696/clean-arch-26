using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CleanArch26.Infrastructure.Persistence;

/// <summary>
/// Used exclusively by EF Core CLI tools at design time (dotnet ef migrations add ...).
/// Allows running migrations without starting the full application.
///
/// Usage:
///   dotnet ef migrations add InitialCreate \
///     --project src/CleanArch26.Infrastructure \
///     --startup-project src/CleanArch26.Api
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Points to the Api project directory so the .db file lands next to the running app
        optionsBuilder.UseSqlite("Data Source=../CleanArch26.Api/cleanarch26.db");

        return new AppDbContext(optionsBuilder.Options);
    }
}
