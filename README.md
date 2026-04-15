# CleanArch26

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
