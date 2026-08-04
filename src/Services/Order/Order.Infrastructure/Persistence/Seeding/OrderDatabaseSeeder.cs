using PizzaSaga.Shared.Infrastructure.Persistence;

namespace Order.Infrastructure.Persistence.Seeding;

public sealed class OrderDatabaseSeeder : IDatabaseSeeder<OrderDbContext>
{
    public async Task SeedAsync(OrderDbContext context, CancellationToken cancellationToken)
    {
        return;

        // Ранний возврат (Early Return) — залог идемпотентности
        //if (await context.Orders.AnyAsync(cancellationToken))
        //{
        //    return;
        //}

        // Заполнение тестовыми данными для Спринта 0
        // var defaultOrders = new[] { ... };
        // await context.Orders.AddRangeAsync(defaultOrders, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
}