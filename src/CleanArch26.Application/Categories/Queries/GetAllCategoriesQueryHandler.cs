using CleanArch26.Application.Categories.Dtos;
using CleanArch26.Application.Interfaces;
using MediatR;

namespace CleanArch26.Application.Categories.Queries;

public sealed class GetAllCategoriesQueryHandler
    : IRequestHandler<GetAllCategoriesQuery, IEnumerable<CategoryDto>>
{
    private readonly ICategoryRepository _repository;

    public GetAllCategoriesQueryHandler(ICategoryRepository repository)
        => _repository = repository;

    public async Task<IEnumerable<CategoryDto>> Handle(
        GetAllCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await _repository.GetAllAsync(cancellationToken);

        // Products are eagerly loaded by the repo so .Count is safe here.
        // For large datasets consider a dedicated COUNT projection instead.
        return categories.Select(c => new CategoryDto(
            c.Id,
            c.Name,
            c.Description,
            c.Products.Count));
    }
}
