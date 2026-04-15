using CleanArch26.Application.Categories.Dtos;
using MediatR;

namespace CleanArch26.Application.Categories.Queries;

public record GetAllCategoriesQuery : IRequest<IEnumerable<CategoryDto>>;
