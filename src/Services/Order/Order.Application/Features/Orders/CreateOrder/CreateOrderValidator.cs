using FluentValidation;

namespace Order.Application.Features.Orders.CreateOrder;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.PaymentMethod)
            .NotEmpty();

        RuleFor(x => x.Currency)
            .NotEmpty();

        RuleForEach(x => x.Items)
            .SetValidator(new CreateOrderItemValidator());
    }
}

internal sealed class CreateOrderItemValidator : AbstractValidator<CreateOrderItem>
{
    public CreateOrderItemValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEqual(Guid.Empty);
    }
}