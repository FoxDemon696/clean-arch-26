using CleanArch26.Application.Orders.Dtos;
using MediatR;

namespace CleanArch26.Application.Orders.Queries;

public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto?>;
