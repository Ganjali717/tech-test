using System;

namespace Order.Service.Exceptions;

public class InvalidOrderStatusTransitionException : Exception
{
    public InvalidOrderStatusTransitionException(string message)
        : base(message)
    {
    }
}