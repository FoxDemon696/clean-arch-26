using CleanArch26.Application.Interfaces;
using CleanArch26.Application.Products.Dtos;
using MediatR;

namespace CleanArch26.Application.Products.Queries;

public sealed class GetAllProductsQueryHandler
    : IRequestHandler<GetAllProductsQuery, IEnumerable<ProductDto>>
{
    private readonly IProductRepository _repository;
    private readonly IExternalInventoryService _inventoryService;

    public GetAllProductsQueryHandler(
        IProductRepository repository,
        IExternalInventoryService inventoryService)
    {
        _repository = repository;
        _inventoryService = inventoryService;
    }

    public async Task<IEnumerable<ProductDto>> Handle(
        GetAllProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await _repository.GetAllAsync(cancellationToken);

        // Enrich each product with stock data; parallelise to avoid serial HTTP waits
        var enriched = await Task.WhenAll(products.Select(async product =>
        {
            var stockLevel = await _inventoryService.GetStockLevelAsync(product.Id, cancellationToken);
            return new ProductDto(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.Category.Name,
                stockLevel);
        }));

        return enriched;
    }
}
