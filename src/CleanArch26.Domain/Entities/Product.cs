namespace CleanArch26.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }

    // Foreign key + navigation to Category entity
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}
