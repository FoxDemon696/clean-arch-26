using CleanArch26.Application.Categories.Dtos;
using CleanArch26.Application.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanArch26.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
        => _mediator = mediator;

    /// <summary>Returns all product categories with their product count.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllCategoriesQuery(), cancellationToken);
        return Ok(result);
    }
}
