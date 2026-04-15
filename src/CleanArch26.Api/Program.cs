using CleanArch26.Application;
using CleanArch26.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// --- Service registration ---
builder.Services.AddApplicationServices();    // MediatR + handlers (Application layer)
builder.Services.AddInfrastructureServices(); // Repositories + external clients (Infrastructure layer)

builder.Services.AddControllers();

var app = builder.Build();

// --- Middleware pipeline ---
app.UseHttpsRedirection();
app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
