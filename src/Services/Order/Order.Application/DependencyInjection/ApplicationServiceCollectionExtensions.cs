using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Order.Application.Behaviors;
using Order.Application.Features.Orders.CreateOrder;

namespace Order.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует зависимости слоя Order.Application.
    /// </summary>
    public static IServiceCollection AddOrderApplication(this IServiceCollection services)
    {
        // Регистрирует все FluentValidation-валидаторы, найденные в сборке Order.Application.
        // Поэтому при добавлении нового Validator отдельную регистрацию в Program.cs делать не нужно.
        services.AddValidatorsFromAssemblyContaining<CreateOrderCommandValidator>();


        // Регистрирует Mediator и application pipeline behaviors.
        services.AddMediator(options =>
        {
            // Mediator и связанные с ним handlers/behaviors
            // регистрируются как Scoped.
            //
            // Это необходимо, поскольку ValidationBehavior
            // использует scoped IValidator<TMessage>.
            //
            // Кроме того, Scoped lifetime хорошо соответствует HTTP request scope и пригодится для TransactionBehavior с EF Core DbContext.
            options.ServiceLifetime = ServiceLifetime.Scoped;

            options.PipelineBehaviors =
            [
                // Логирование всех сообщений Mediator.
                typeof(LoggingBehavior<,>),

                // Валидация только сообщений, реализующих ICommand<TResponse>.
                typeof(ValidationBehavior<,>)
            ];
        });

        return services;
    }
}
