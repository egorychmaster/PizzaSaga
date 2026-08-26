using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Order.Application.Abstractions.Persistence.Idempotency;
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
        services.AddValidatorsFromAssemblyContaining<CreateOrderCommandValidator>();

        // Регистрируем scoped контекст идемпотентности (для передачи параметров из API в Application)
        services.AddScoped<IIdempotencyContext, IdempotencyContext>();

        // Регистрируем Mediator с pipeline behaviors.
        //
        // ВАЖНО: порядок поведений определяет порядок выполнения:
        //   LoggingBehavior → ValidationBehavior → IdempotencyBehavior → TransactionBehavior → Handler
        //
        // Это соответствует архитектуре:
        //   IdempotencyBehavior (внешний) перехватывает DuplicateIdempotencyKeyException после ROLLBACK.
        //   TransactionBehavior (внутренний) управляет транзакцией и SaveChanges/Commit.
        //
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;

            options.PipelineBehaviors =
            [
                // Логирование всех сообщений.
                typeof(LoggingBehavior<,>),
                // Валидация только сообщений, реализующих ICommand<TResponse>.
                typeof(ValidationBehavior<,>),

                // IdempotencyBehavior должен быть ВНЕШНИМ (выполняется первым из behavior-контейнера).
                typeof(IdempotencyBehavior<,>),
                // TransactionBehavior — внутренний: управляет транзакцией и SaveChanges.
                // Автоматическое управление транзакциями БД для команд.
                typeof(TransactionBehavior<,>)
            ];
        });

        return services;
    }
}