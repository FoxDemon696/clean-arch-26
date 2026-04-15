# CleanArch26

A .NET 8 Web API built on **Clean Architecture** principles. The solution is split into four layers with strict, one-directional dependencies. MediatR handles in-process CQRS and EF Core (SQLite) provides persistence.

---

## Project Structure

```
src/
├── CleanArch26.Domain/
│   ├── Entities/
│   │   ├── Category.cs
│   │   ├── Product.cs
│   │   ├── Order.cs
│   │   └── OrderItem.cs
│   └── Enums/
│       └── OrderStatus.cs
│
├── CleanArch26.Application/
│   ├── Interfaces/
│   │   ├── IProductRepository.cs
│   │   ├── ICategoryRepository.cs
│   │   ├── IOrderRepository.cs
│   │   └── IExternalInventoryService.cs
│   ├── Products/
│   │   ├── Dtos/ProductDto.cs
│   │   └── Queries/
│   │       ├── GetAllProductsQuery.cs / Handler
│   │       └── GetProductByIdQuery.cs / Handler
│   ├── Categories/
│   │   ├── Dtos/CategoryDto.cs
│   │   └── Queries/
│   │       └── GetAllCategoriesQuery.cs / Handler
│   ├── Orders/
│   │   ├── Dtos/OrderDto.cs + OrderItemDto.cs
│   │   └── Queries/
│   │       └── GetOrderByIdQuery.cs / Handler
│   └── DependencyInjection.cs
│
├── CleanArch26.Infrastructure/
│   ├── Persistence/
│   │   ├── AppDbContext.cs
│   │   ├── AppDbContextFactory.cs       ← design-time factory for EF CLI
│   │   ├── Configurations/
│   │   │   ├── CategoryConfiguration.cs
│   │   │   ├── ProductConfiguration.cs
│   │   │   ├── OrderConfiguration.cs
│   │   │   └── OrderItemConfiguration.cs
│   │   └── Seed/
│   │       └── DatabaseSeeder.cs
│   ├── Repositories/
│   │   ├── ProductRepository.cs
│   │   ├── CategoryRepository.cs
│   │   └── OrderRepository.cs
│   ├── ExternalServices/
│   │   └── ExternalInventoryServiceClient.cs
│   └── DependencyInjection.cs
│
└── CleanArch26.Api/
    ├── Controllers/
    │   ├── ProductsController.cs
    │   ├── CategoriesController.cs
    │   └── OrdersController.cs
    ├── appsettings.json                  ← connection string lives here
    └── Program.cs
```

---

## Domain Model

```
┌────────────┐        ┌─────────────┐
│  Category  │1──────*│   Product   │
│────────────│        │─────────────│
│ Id         │        │ Id          │
│ Name       │        │ Name        │
│ Description│        │ Description │
└────────────┘        │ Price       │
                      │ CategoryId  │
                      └──────┬──────┘
                             │ *
                    ┌────────┴────────┐
                    │   OrderItem     │
                    │─────────────────│
                    │ Id              │
                    │ OrderId         │
                    │ ProductId       │
                    │ Quantity        │
              *     │ UnitPrice       │
    ┌───────────────┤                 │
    │    Order      └─────────────────┘
    │───────────────
    │ Id
    │ CustomerName
    │ CustomerEmail
    │ OrderDate
    │ Status (enum)
    │ TotalAmount (computed)
    └───────────────
```

`OrderStatus` enum values: `Pending · Confirmed · Shipped · Delivered · Cancelled`

---

## Layer Responsibilities

### Domain
Plain C# entities and enums. No NuGet dependencies, no I/O. Business rules and value objects live here.

### Application
Orchestrates use cases. Contains:
- **Interfaces** – contracts for repositories and external services. Defined here so Application never depends on Infrastructure.
- **Queries / Commands** – MediatR `IRequest<T>` records.
- **Handlers** – `IRequestHandler<TRequest, TResponse>` combine repository data (and optionally external service data) and return DTOs.
- **DTOs** – data shapes returned to the API caller.

No knowledge of HTTP, databases, or third-party APIs.

### Infrastructure
Implements the interfaces defined in Application:
- **AppDbContext** – EF Core DbContext; entity configurations auto-discovered via `ApplyConfigurationsFromAssembly`.
- **Repositories** – EF Core implementations; use `.Include()` for eager loading of navigation properties.
- **ExternalServices** – typed `HttpClient` adapters. Mock responses keep the system runnable without a live dependency.

This is the **only** layer allowed to do I/O (database, HTTP, file system, queues).

### Api (Presentation)
- **Controllers** – thin; dispatch MediatR queries/commands and map results to HTTP responses.
- **Program.cs** – registers Application + Infrastructure services and creates the DB schema on startup.

Controllers never reference repositories or external services directly.

---

## Database Setup

### Connection string

Edit `src/CleanArch26.Api/appsettings.json`:

```json
"ConnectionStrings": {
  "Default": "Data Source=cleanarch26.db"
}
```

| Provider | Connection string |
|---|---|
| **SQLite** (default, no server needed) | `Data Source=cleanarch26.db` |
| **SQL Server** | `Server=.;Database=CleanArch26;Trusted_Connection=True;` |
| **PostgreSQL** (needs `Npgsql.EntityFrameworkCore.PostgreSQL` package) | `Host=localhost;Database=cleanarch26;Username=postgres;Password=secret` |

To switch providers, change the `UseSqlite(...)` call in `Infrastructure/DependencyInjection.cs` to `UseSqlServer(...)` or `UseNpgsql(...)`.

### Schema creation (development)

On startup, `Program.cs` calls `EnsureCreatedAsync()`, which creates the full schema from the model in one step — no migration files needed during early development.

```csharp
await db.Database.EnsureCreatedAsync(); // creates schema if missing
await DatabaseSeeder.SeedAsync(db);     // inserts reference data if empty
```

> `EnsureCreated` does **not** support incremental schema changes. Switch to migrations before your first schema change.

### Migrations (recommended for production)

Install the EF Core global CLI tool once:

```bash
dotnet tool install --global dotnet-ef
```

**Create a migration:**

```bash
dotnet ef migrations add InitialCreate \
  --project src/CleanArch26.Infrastructure \
  --startup-project src/CleanArch26.Api
```

**Apply migrations to the database:**

```bash
dotnet ef database update \
  --project src/CleanArch26.Infrastructure \
  --startup-project src/CleanArch26.Api
```

**In Program.cs, replace `EnsureCreatedAsync` with `MigrateAsync`:**

```csharp
// Before (development only):
await db.Database.EnsureCreatedAsync();

// After (production-safe):
await db.Database.MigrateAsync();
```

Migrations are stored in `src/CleanArch26.Infrastructure/Migrations/` and should be committed to source control.

### Adding a new entity to the database

1. **Domain** – create the entity class in `Domain/Entities/`.
2. **Infrastructure/Persistence/AppDbContext** – add a `DbSet<YourEntity>`.
3. **Infrastructure/Persistence/Configurations** – add `YourEntityConfiguration : IEntityTypeConfiguration<YourEntity>` (auto-discovered).
4. **Run migration** – `dotnet ef migrations add AddYourEntity ...`.
5. **Seed** – optionally add rows in `DatabaseSeeder.SeedAsync`.

---

## Dependency Flow

```
Domain  ←  Application  ←  Infrastructure
                ↑                  ↑
                └──────  Api  ──────
```

Each arrow means "depends on". Domain has zero outward dependencies. Api wires everything at startup but contains no business logic.

---

## Request Scaffolding

```
HTTP Request
    │
    ▼
[Controller]  (Api)
  Calls ISender.Send(new GetProductByIdQuery(id))
    │
    ▼
[Query]  (Application)
  Record implementing IRequest<ProductDto?>
    │
    ▼
[Handler]  (Application)
  Calls IProductRepository  ──────────────────▶  [ProductRepository]  (Infrastructure)
                                                    └─ AppDbContext.Products.Include(Category)
  Calls IExternalInventoryService  ────────────▶  [ExternalInventoryServiceClient]  (Infrastructure)
  Builds and returns ProductDto
    │
    ▼
[Controller]
  Returns Ok(productDto) / NotFound()
    │
    ▼
HTTP Response
```

---

## Running the API

```bash
dotnet run --project src/CleanArch26.Api
```

The SQLite database file (`cleanarch26.db`) is created automatically on first run and seeded with sample data.

### Available endpoints

| Method | URL | Description |
|--------|-----|-------------|
| `GET` | `/api/products` | All products with category name and stock level |
| `GET` | `/api/products/{id}` | Single product by GUID |
| `GET` | `/api/categories` | All categories with product count |
| `GET` | `/api/orders/{id}` | Single order with line items |

### Sample seed GUIDs

| Entity | GUID |
|--------|------|
| Category: Electronics | `22222222-0000-0000-0000-000000000001` |
| Category: Accessories | `22222222-0000-0000-0000-000000000002` |
| Product: Laptop Pro 15 | `11111111-0000-0000-0000-000000000001` |
| Product: Wireless Keyboard | `11111111-0000-0000-0000-000000000002` |
| Product: 4K Monitor 27" | `11111111-0000-0000-0000-000000000003` |
| Order (Jane Doe) | `33333333-0000-0000-0000-000000000001` |

---

## Adding a New Feature

Follow these steps to add a new endpoint – e.g. "create a product":

### 1. Application — add the command and handler

**`Application/Products/Commands/CreateProductCommand.cs`**
```csharp
public record CreateProductCommand(string Name, string Description, decimal Price, Guid CategoryId)
    : IRequest<Guid>;
```

**`Application/Products/Commands/CreateProductCommandHandler.cs`**
```csharp
public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository)
        => _repository = repository;

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product { Id = Guid.NewGuid(), Name = request.Name, /* … */ };
        await _repository.AddAsync(product, cancellationToken);
        return product.Id;
    }
}
```

### 2. Application — extend the interface

```csharp
// IProductRepository.cs
Task AddAsync(Product product, CancellationToken cancellationToken = default);
```

### 3. Infrastructure — implement the new method

```csharp
// ProductRepository.cs
public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
{
    await _context.Products.AddAsync(product, cancellationToken);
    await _context.SaveChangesAsync(cancellationToken);
}
```

### 4. Api — add the controller action

```csharp
[HttpPost]
[ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
public async Task<IActionResult> Create(CreateProductCommand command, CancellationToken cancellationToken)
{
    var id = await _mediator.Send(command, cancellationToken);
    return CreatedAtAction(nameof(GetById), new { id }, id);
}
```

---

## Key Packages

| Package | Layer | Purpose |
|---------|-------|---------|
| `MediatR` 12 | Application, Api | In-process CQRS message bus |
| `Microsoft.EntityFrameworkCore.Sqlite` | Infrastructure | EF Core SQLite provider |
| `Microsoft.EntityFrameworkCore.Design` | Infrastructure | EF Core CLI migration tooling |
| `Microsoft.Extensions.Http` | Infrastructure | Typed `HttpClient` factory |


A .NET 8 Web API built on **Clean Architecture** principles. The solution is split into four layers with strict, one-directional dependencies. MediatR is used as the in-process message bus to decouple the API from business logic.

---

## Project Structure

```
src/
├── CleanArch26.Domain/
│   └── Entities/
│       └── Product.cs
│
├── CleanArch26.Application/
│   ├── Interfaces/
│   │   ├── IProductRepository.cs
│   │   └── IExternalInventoryService.cs
│   ├── Products/
│   │   ├── Dtos/
│   │   │   └── ProductDto.cs
│   │   └── Queries/
│   │       ├── GetAllProductsQuery.cs
│   │       ├── GetAllProductsQueryHandler.cs
│   │       ├── GetProductByIdQuery.cs
│   │       └── GetProductByIdQueryHandler.cs
│   └── DependencyInjection.cs
│
├── CleanArch26.Infrastructure/
│   ├── Repositories/
│   │   └── ProductRepository.cs
│   ├── ExternalServices/
│   │   └── ExternalInventoryServiceClient.cs
│   └── DependencyInjection.cs
│
└── CleanArch26.Api/
    ├── Controllers/
    │   └── ProductsController.cs
    └── Program.cs
```

---

## Layer Responsibilities

### Domain
The innermost layer. Contains **plain C# entities** with no dependencies on any other layer or NuGet package. Business rules and value objects live here.

### Application
Orchestrates use cases. Contains:
- **Interfaces** – contracts for the repository and external services. Defined here so that Application never depends on Infrastructure.
- **Queries / Commands** – MediatR `IRequest<T>` records that describe _what_ should happen.
- **Handlers** – `IRequestHandler<TRequest, TResponse>` classes that implement the use case by combining repository data with external service calls.
- **DTOs** – data shapes returned to the caller.

This layer has no knowledge of HTTP, databases, or third-party APIs.

### Infrastructure
Implements the interfaces defined in Application:
- **Repositories** – data access (in-memory stub; swap for EF Core against a real database in production).
- **ExternalServices** – typed `HttpClient` adapters for third-party APIs. The mock responses in `ExternalInventoryServiceClient` let the rest of the system run without a live dependency.

This is the **only** correct place for I/O concerns (database, HTTP, file system, message queues).

### Api (Presentation)
The entry point. Contains:
- **Controllers** – thin; map HTTP requests to MediatR queries/commands and return the appropriate HTTP response.
- **Program.cs** – registers services from Application and Infrastructure and builds the middleware pipeline.

Controllers never reference repositories or external services directly; they only know `IMediator`.

---

## Dependency Flow

```
Domain  ←  Application  ←  Infrastructure
                ↑                  ↑
                └──────  Api  ──────
```

Each arrow means "depends on". `Domain` has no outward dependencies. `Api` wires everything together at startup but contains no business logic.

---

## Request Scaffolding

A request travels through the following layers:

```
HTTP Request
    │
    ▼
[Controller]  (Api)
  Calls ISender.Send(new GetProductByIdQuery(id))
    │
    ▼
[Query]  (Application)
  Record implementing IRequest<ProductDto?>
    │
    ▼
[Handler]  (Application)
  Calls IProductRepository  ──────────────────▶  [ProductRepository]  (Infrastructure)
  Calls IExternalInventoryService  ────────────▶  [ExternalInventoryServiceClient]  (Infrastructure)
  Combines results into ProductDto
    │
    ▼
[Controller]
  Returns Ok(productDto) / NotFound()
    │
    ▼
HTTP Response
```

---

## Adding a New Feature

Follow these steps to add a new endpoint – e.g. "create a product":

### 1. Domain — add or update an entity (if needed)
No changes needed for a simple create.

### 2. Application — add the command and handler

**`Application/Products/Commands/CreateProductCommand.cs`**
```csharp
public record CreateProductCommand(string Name, string Description, decimal Price, string Category)
    : IRequest<Guid>;
```

**`Application/Products/Commands/CreateProductCommandHandler.cs`**
```csharp
public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository)
        => _repository = repository;

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product { Id = Guid.NewGuid(), Name = request.Name, /* … */ };
        await _repository.AddAsync(product, cancellationToken);
        return product.Id;
    }
}
```

### 3. Application — extend the interface

```csharp
// IProductRepository.cs
Task AddAsync(Product product, CancellationToken cancellationToken = default);
```

### 4. Infrastructure — implement the new interface method

```csharp
// ProductRepository.cs
public Task AddAsync(Product product, CancellationToken cancellationToken = default)
{
    _products.Add(product);
    return Task.CompletedTask;
}
```

### 5. Api — add the controller action

```csharp
[HttpPost]
[ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
public async Task<IActionResult> Create(CreateProductCommand command, CancellationToken cancellationToken)
{
    var id = await _mediator.Send(command, cancellationToken);
    return CreatedAtAction(nameof(GetById), new { id }, id);
}
```

---

## Running the API

```bash
dotnet run --project src/CleanArch26.Api
```

Default endpoints:

| Method | URL | Description |
|--------|-----|-------------|
| `GET` | `/api/products` | Returns all products with stock levels |
| `GET` | `/api/products/{id}` | Returns a single product by GUID |

---

## Key Packages

| Package | Layer | Purpose |
|---------|-------|---------|
| `MediatR` 12 | Application, Api | In-process CQRS message bus |
| `Microsoft.Extensions.Http` | Infrastructure | Typed `HttpClient` factory |
