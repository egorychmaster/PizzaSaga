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
            ProductAggregate.Create(Guid.NewGuid(), "Margherita", "Classic pizza with tomato sauce, mozzarella, and basil."),
            ProductAggregate.Create(Guid.NewGuid(), "Pepperoni", "Pizza topped with pepperoni slices and cheese."),
            ProductAggregate.Create(Guid.NewGuid(), "Hawaiian", "Pizza with ham and pineapple."),
            ProductAggregate.Create(Guid.NewGuid(), "Veggie Supreme", "Loaded with bell peppers, mushrooms, onions, and olives."),
            ProductAggregate.Create(Guid.NewGuid(), "BBQ Chicken", "Grilled chicken, BBQ sauce, red onions, and cilantro."),
            ProductAggregate.Create(Guid.NewGuid(), "Four Cheese", "Mozzarella, gorgonzola, parmesan, and provolone."),
            ProductAggregate.Create(Guid.NewGuid(), "Meat Feast", "Pepperoni, ham, bacon, and sausage."),
            ProductAggregate.Create(Guid.NewGuid(), "Spicy Marbouha", "Spicy lamb, onions, peppers, and harissa sauce."),
            ProductAggregate.Create(Guid.NewGuid(), "Tropical", "Pineapple, coconut, and banana."),
            ProductAggregate.Create(Guid.NewGuid(), "Buffalo Chicken", "Spicy buffalo sauce, chicken, jalapeños, and blue cheese.")
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

        await context.Products.AddRangeAsync(products, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
}