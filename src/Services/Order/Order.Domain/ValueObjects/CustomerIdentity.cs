namespace Order.Domain.ValueObjects;

/// <summary>
/// Идентификатор клиента (GUID), исключающий пустое значение.
/// </summary>
public sealed class CustomerIdentity
{
    /// <summary>
    /// Значение идентификатора. Не может быть Guid.Empty.
    /// </summary>
    public Guid Value { get; }

    private CustomerIdentity(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Customer ID cannot be empty.", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Фабричный метод для создания CustomerIdentity из GUID.
    /// </summary>
    public static CustomerIdentity Create(Guid id) => new(id);
}