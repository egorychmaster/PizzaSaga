using PizzaSaga.SharedKernel.Domain.Exceptions;

namespace Catalog.Domain.AggregatesModel.Products.Exceptions.ProductAggregates;

public sealed class InvalidProductNameException : DomainException
{
    public InvalidProductNameException() : base("Product name is required.") { }
}