using System;
using System.Collections.Generic;
using System.Linq;
using Order.Service.Exceptions;

namespace Order.Service.Status
{
    public static class OrderStatusTransitions
    {
        private static readonly Dictionary<string, string[]> AllowedTransitions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                [OrderStatuses.Created] = new[]
                {
                    OrderStatuses.InProgress,
                    OrderStatuses.Failed
                },
                [OrderStatuses.InProgress] = new[]
                {
                    OrderStatuses.Completed,
                    OrderStatuses.Failed
                },
                [OrderStatuses.Failed] = new[]
                {
                    OrderStatuses.InProgress
                },
                [OrderStatuses.Completed] = Array.Empty<string>()
            };

        public static void Validate(string current, string target)
        {
            if (string.IsNullOrWhiteSpace(current))
                throw new ArgumentException("Current status is required.", nameof(current));

            if (string.IsNullOrWhiteSpace(target))
                throw new ArgumentException("Target status is required.", nameof(target));

            if (!AllowedTransitions.TryGetValue(current, out var allowed))
            {
                throw new InvalidOrderStatusTransitionException(
                    $"Unknown current status '{current}'.");
            }

            if (!allowed.Contains(target, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOrderStatusTransitionException(
                    $"Cannot change status from '{current}' to '{target}'.");
            }
        }
    }
}