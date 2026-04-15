using CleanArch26.Application.Orders.Dtos;
using CleanArch26.Application.Orders.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanArch26.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
        => _mediator = mediator;

    /// <summary>Returns a single order with its line items.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
