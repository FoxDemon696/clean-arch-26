using CleanArch26.Application.Interfaces;
using CleanArch26.Application.Orders.Dtos;
using MediatR;

namespace CleanArch26.Application.Orders.Queries;

public sealed class GetOrderByIdQueryHandler
    : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IOrderRepository _repository;

    public GetOrderByIdQueryHandler(IOrderRepository repository)
        => _repository = repository;

    public async Task<OrderDto?> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order is null)
            return null;

        var items = order.Items.Select(i => new OrderItemDto(
            i.Id,
            i.ProductId,
            i.Product.Name,
            i.Quantity,
            i.UnitPrice,
            i.UnitPrice * i.Quantity));

        return new OrderDto(
            order.Id,
            order.CustomerName,
            order.CustomerEmail,
            order.OrderDate,
            order.Status.ToString(),
            items,
            order.TotalAmount);
    }
}
