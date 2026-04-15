using CleanArch26.Application.Products.Dtos;
using MediatR;

namespace CleanArch26.Application.Products.Queries;

/// <summary>Query to retrieve a single product by its identifier, enriched with live stock level.</summary>
public record GetProductByIdQuery(Guid ProductId) : IRequest<ProductDto?>;
