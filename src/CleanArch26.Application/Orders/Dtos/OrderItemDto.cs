namespace CleanArch26.Application.Orders.Dtos;

public record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal
);
