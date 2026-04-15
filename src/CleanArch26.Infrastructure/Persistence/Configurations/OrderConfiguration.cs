using CleanArch26.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArch26.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.CustomerName).IsRequired().HasMaxLength(200);
        builder.Property(o => o.CustomerEmail).IsRequired().HasMaxLength(254);
        builder.Property(o => o.OrderDate).IsRequired();

        // Store the enum as a string for readability in the database
        builder.Property(o => o.Status)
               .HasConversion<string>()
               .HasMaxLength(20);

        // Computed property – not persisted
        builder.Ignore(o => o.TotalAmount);
    }
}
