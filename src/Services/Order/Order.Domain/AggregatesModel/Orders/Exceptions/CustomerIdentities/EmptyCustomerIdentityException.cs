using PizzaSaga.SharedKernel.Domain.Exceptions;

namespace Order.Domain.AggregatesModel.Orders.Exceptions.CustomerIdentities;

public sealed class EmptyCustomerIdentityException : DomainException
{
    public EmptyCustomerIdentityException()
        : base("Customer ID cannot be empty.") { }
}
