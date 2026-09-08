using Catalog.Domain.AggregatesModel.Products;
using Microsoft.EntityFrameworkCore;
using PizzaSaga.Shared.Infrastructure.Persistence;

namespace Catalog.Infrastructure.Persistence.Seeding;

public sealed class CatalogDatabaseSeeder : IDatabaseSeeder<CatalogDbContext>
{
    public async Task SeedAsync(CatalogDbContext context, CancellationToken cancellationToken)
    {
        //return;

        // Ранний возврат (Early Return) — залог идемпотентности
        if (await context.Products.AnyAsync(cancellationToken))
            return;

        // Заполнение тестовыми данными
        var products = new List<ProductAggregate>
        {
            ProductAggregate.Create(Guid.NewGuid(), "Margherita", "Classic pizza with tomato sauce, mozzarella, and basil.", 10.0m, "USD"),
            ProductAggregate.Create(Guid.NewGuid(), "Pepperoni", "Pizza topped with pepperoni slices and cheese.", 12.0m, "USD"),
            ProductAggregate.Create(Guid.NewGuid(), "Hawaiian", "Pizza with ham and pineapple.", 11.5m, "USD"),
            ProductAggregate.Create(Guid.NewGuid(), "Veggie Supreme", "Loaded with bell peppers, mushrooms, onions, and olives.", 11.0m, "USD"),
            ProductAggregate.Create(Guid.NewGuid(), "BBQ Chicken", "Grilled chicken, BBQ sauce, red onions, and cilantro.", 13.0m, "USD"),
            ProductAggregate.Create(Guid.NewGuid(), "Four Cheese", "Mozzarella, gorgonzola, parmesan, and provolone.", 12.5m, "USD"),
            ProductAggregate.Create(Guid.NewGuid(), "Meat Feast", "Pepperoni, ham, bacon, and sausage.", 14.0m, "USD"),
            ProductAggregate.Create(Guid.NewGuid(), "Spicy Marbouha", "Spicy lamb, onions, peppers, and harissa sauce.", 13.5m, "USD"),
            ProductAggregate.Create(Guid.NewGuid(), "Tropical", "Pineapple, coconut, and banana.", 10.5m, "USD"),
            ProductAggregate.Create(Guid.NewGuid(), "Buffalo Chicken", "Spicy buffalo sauce, chicken, jalapeños, and blue cheese.", 12.0m, "USD")
        };

        await context.Products.AddRangeAsync(products, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
}