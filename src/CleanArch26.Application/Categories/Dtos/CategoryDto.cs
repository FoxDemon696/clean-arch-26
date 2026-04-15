namespace CleanArch26.Application.Categories.Dtos;

public record CategoryDto(
    Guid Id,
    string Name,
    string Description,
    int ProductCount
);
