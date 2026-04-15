using CleanArch26.Application;
using CleanArch26.Infrastructure;
using CleanArch26.Infrastructure.Persistence;
using CleanArch26.Infrastructure.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);

// --- Service registration ---
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Data Source=cleanarch26.db";

builder.Services.AddApplicationServices();               // MediatR + handlers
builder.Services.AddInfrastructureServices(connectionString); // EF Core + repositories + external clients
builder.Services.AddControllers();

var app = builder.Build();

// --- Schema creation & seeding (development-friendly) ---
// EnsureCreated creates the schema from the current model without requiring migrations.
// Switch to `await db.Database.MigrateAsync()` once you have generated migrations.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DatabaseSeeder.SeedAsync(db);
}

// --- Middleware pipeline ---
app.UseHttpsRedirection();
app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
