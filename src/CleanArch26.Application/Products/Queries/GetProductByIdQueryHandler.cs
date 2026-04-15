using CleanArch26.Application.Interfaces;
using CleanArch26.Application.Products.Dtos;
using MediatR;

namespace CleanArch26.Application.Products.Queries;

public sealed class GetProductByIdQueryHandler
    : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IProductRepository _repository;
    private readonly IExternalInventoryService _inventoryService;

    public GetProductByIdQueryHandler(
        IProductRepository repository,
        IExternalInventoryService inventoryService)
    {
        _repository = repository;
        _inventoryService = inventoryService;
    }

    public async Task<ProductDto?> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
            return null;

        // Enrich with live stock data from the external inventory system
        var stockLevel = await _inventoryService.GetStockLevelAsync(product.Id, cancellationToken);

        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Category.Name,
            stockLevel);
    }
}
