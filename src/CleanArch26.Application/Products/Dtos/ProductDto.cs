namespace CleanArch26.Application.Products.Dtos;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string CategoryName,
    int StockLevel
);
