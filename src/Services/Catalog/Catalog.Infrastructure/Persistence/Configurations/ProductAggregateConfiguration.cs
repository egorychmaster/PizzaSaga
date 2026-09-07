using Catalog.Domain.AggregatesModel.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

public class ProductAggregateConfiguration : IEntityTypeConfiguration<ProductAggregate>
{
    public void Configure(EntityTypeBuilder<ProductAggregate> builder)
    {
        // Primary key
        builder.HasKey(p => p.Id);

        // Columns
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);

        builder.Property(p => p.Description).HasMaxLength(500);

        //// Value object Price — owned entity
        // Owned-тип
        builder.OwnsOne(
            p => p.CurrentPrice,
            price =>
            {
                // Columns
                price.Property(p => p.Amount).HasColumnType("decimal(18,2)").IsRequired();
                price.Property(p => p.CurrencyCode).HasMaxLength(3).IsRequired();

                price.ToTable("Prices");
            }
            );

        //builder.HasData(GetSeededProducts());

        // Maps to table
        builder.ToTable("Products");
    }

    private static IEnumerable<ProductAggregate> GetSeededProducts()
    {
        var products = new List<ProductAggregate>
        {
            ProductAggregate.Create(Guid.NewGuid(), "Margherita", "Classic pizza with tomato sauce, mozzarella, and basil.", "/images/pizza-margherita.jpg"),
            ProductAggregate.Create(Guid.NewGuid(), "Pepperoni", "Pizza topped with pepperoni slices and cheese.", "/images/pizza-pepperoni.jpg"),
            ProductAggregate.Create(Guid.NewGuid(), "Hawaiian", "Pizza with ham and pineapple.", "/images/pizza-hawaiian.jpg"),
            ProductAggregate.Create(Guid.NewGuid(), "Veggie Supreme", "Loaded with bell peppers, mushrooms, onions, and olives.", "/images/pizza-veggie-supreme.jpg"),
            ProductAggregate.Create(Guid.NewGuid(), "BBQ Chicken", "Grilled chicken, BBQ sauce, red onions, and cilantro.", "/images/pizza-bbq-chicken.jpg"),
            ProductAggregate.Create(Guid.NewGuid(), "Four Cheese", "Mozzarella, gorgonzola, parmesan, and provolone.", "/images/pizza-four-cheese.jpg"),
            ProductAggregate.Create(Guid.NewGuid(), "Meat Feast", "Pepperoni, ham, bacon, and sausage.", "/images/pizza-meat-feast.jpg"),
            ProductAggregate.Create(Guid.NewGuid(), "Spicy Marbouha", "Spicy lamb, onions, peppers, and harissa sauce.", "/images/pizza-marbouha.jpg"),
            ProductAggregate.Create(Guid.NewGuid(), "Tropical", "Pineapple, coconut, and banana.", "/images/pizza-tropical.jpg"),
            ProductAggregate.Create(Guid.NewGuid(), "Buffalo Chicken", "Spicy buffalo sauce, chicken, jalapeños, and blue cheese.", "/images/pizza-buffalo-chicken.jpg")
        };

        // Устанавливаем текущие цены в USD
        products[0].SetCurrentPrice(10.0m, "USD");
        products[1].SetCurrentPrice(12.0m, "USD");
        products[2].SetCurrentPrice(11.5m, "USD");
        products[3].SetCurrentPrice(11.0m, "USD");
        products[4].SetCurrentPrice(13.0m, "USD");
        products[5].SetCurrentPrice(12.5m, "USD");
        products[6].SetCurrentPrice(14.0m, "USD");
        products[7].SetCurrentPrice(13.5m, "USD");
        products[8].SetCurrentPrice(10.5m, "USD");
        products[9].SetCurrentPrice(12.0m, "USD");

        return products;
    }
}