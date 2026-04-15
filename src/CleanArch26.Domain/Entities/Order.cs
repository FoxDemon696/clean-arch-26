using CleanArch26.Domain.Enums;

namespace CleanArch26.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }

    // Navigation property
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    // Computed – not stored in DB (configured via Ignore in OrderConfiguration)
    public decimal TotalAmount => Items.Sum(i => i.UnitPrice * i.Quantity);
}
