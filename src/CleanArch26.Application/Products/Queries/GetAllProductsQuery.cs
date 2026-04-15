using CleanArch26.Application.Products.Dtos;
using MediatR;

namespace CleanArch26.Application.Products.Queries;

/// <summary>Query to retrieve all products, each enriched with a live stock level.</summary>
public record GetAllProductsQuery : IRequest<IEnumerable<ProductDto>>;
