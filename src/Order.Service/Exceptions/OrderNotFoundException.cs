using System;

namespace Order.Service.Exceptions;

public class OrderNotFoundException : Exception
{
    public OrderNotFoundException(Guid id)
        : base($"Order '{id}' not found.")
    {
    }
}