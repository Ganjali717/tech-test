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
                ["Created"] = new[] { "In Progress", "Failed" },
                ["In Progress"] = new[] { "Completed", "Failed" },
                ["Failed"] = new[] { "In Progress" },
                ["Completed"] = Array.Empty<string>()
            };

        public static void Validate(string current, string target)
        {
            if (!AllowedTransitions.TryGetValue(current, out var allowed))
            {
                throw new InvalidOrderStatusTransitionException(
                    $"Unknown current status '{current}'.");
            }

            if (!allowed.Contains(target))
            {
                throw new InvalidOrderStatusTransitionException(
                    $"Cannot change status from '{current}' to '{target}'.");
            }
        }
    }
}