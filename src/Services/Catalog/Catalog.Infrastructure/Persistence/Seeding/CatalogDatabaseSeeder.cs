using Catalog.Application.Abstractions.Persistence;
using Catalog.Domain.AggregatesModel.Products;
using Microsoft.EntityFrameworkCore;
using PizzaSaga.Shared.Infrastructure.Persistence;

namespace Catalog.Infrastructure.Persistence.Seeding;

public sealed class CatalogDatabaseSeeder : IDatabaseSeeder<CatalogDbContext>
{
    private readonly IUnitOfWork _unitOfWork;

    public CatalogDatabaseSeeder(IUnitOfWork unitOfWork)
        => _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    public async Task SeedAsync(CatalogDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Products.AnyAsync(cancellationToken))
            return;

        // Создание продуктов
        var products = new List<ProductAggregate>
        {
            ProductAggregate.Create(Guid.Parse("eba653b0-f324-47d9-947b-99e76c7d6a1c"), "Margherita", "Classic pizza with tomato sauce, mozzarella, and basil.", 10.0m, "USD"),
            ProductAggregate.Create(Guid.Parse("e6d1c383-353e-4b41-9a1f-5b3cc17dc431"), "Pepperoni", "Pizza topped with pepperoni slices and cheese.", 12.0m, "USD"),
            ProductAggregate.Create(Guid.Parse("e7ca7f19-df5d-4d3f-b241-d387641ad9b0"), "Hawaiian", "Pizza with ham and pineapple.", 11.5m, "USD"),
            ProductAggregate.Create(Guid.Parse("a0a7e3fa-2a4d-4fa7-b1f8-6d68c7b98359"), "Veggie Supreme", "Loaded with bell peppers, mushrooms, onions, and olives.", 11.0m, "USD"),
            ProductAggregate.Create(Guid.Parse("d2492a66-0a43-4b1c-8fae-09ca51223518"), "BBQ Chicken", "Grilled chicken, BBQ sauce, red onions, and cilantro.", 13.0m, "USD"),
            ProductAggregate.Create(Guid.Parse("836afed2-bde4-4913-90f8-09d2d6d8ee90"), "Four Cheese", "Mozzarella, gorgonzola, parmesan, and provolone.", 12.5m, "USD"),
            ProductAggregate.Create(Guid.Parse("38fcc875-0570-45d5-837f-09c951ff7244"), "Meat Feast", "Pepperoni, ham, bacon, and sausage.", 14.0m, "USD"),
            ProductAggregate.Create(Guid.Parse("53706345-56ff-4154-9d24-d730502d56ea"), "Spicy Marbouha", "Spicy lamb, onions, peppers, and harissa sauce.", 13.5m, "USD"),
            ProductAggregate.Create(Guid.Parse("c63f9679-fd01-46b9-8cae-dd010a57ad1b"), "Tropical", "Pineapple, coconut, and banana.", 10.5m, "USD"),
            ProductAggregate.Create(Guid.Parse("5887c2c9-9781-4177-8273-590cd5a86e7f"), "Buffalo Chicken", "Spicy buffalo sauce, chicken, jalapeños, and blue cheese.", 12.0m, "USD")
        };

        foreach (var product in products)
        {
            await _unitOfWork.SaveWithOutboxAsync(product, cancellationToken);
        }
    }
}