namespace CleanArch26.Application.Orders.Dtos;

public record OrderDto(
    Guid Id,
    string CustomerName,
    string CustomerEmail,
    DateTime OrderDate,
    string Status,
    IEnumerable<OrderItemDto> Items,
    decimal TotalAmount
);
