using PizzaSaga.SharedKernel.Domain.Exceptions;

namespace Order.Domain.Exceptions.CustomerIdentities;

public sealed class EmptyCustomerIdentityException : DomainException
{
    public EmptyCustomerIdentityException()
        : base("Customer ID cannot be empty.") { }
}
