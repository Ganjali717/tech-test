using System;
using System.Collections.Generic;
using System.Linq;

namespace Order.Service.Status
{
    public interface IOrderStatusNormalizer
    {
        string Normalize(string status);
    }

    public sealed class OrderStatusNormalizer : IOrderStatusNormalizer
    {
        private static readonly Dictionary<string, string> Aliases =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["created"] = OrderStatuses.Created,
                ["inprogress"] = OrderStatuses.InProgress,
                ["in_progress"] = OrderStatuses.InProgress,
                ["in progress"] = OrderStatuses.InProgress,
                ["completed"] = OrderStatuses.Completed,
                ["failed"] = OrderStatuses.Failed
            };

        public string Normalize(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status is required.", nameof(status));

            var trimmed = status.Trim();
            var key = trimmed.Replace(" ", string.Empty);

            if (Aliases.TryGetValue(key, out var canonical))
                return canonical;

            var match = OrderStatuses.All
                .FirstOrDefault(s => s.Equals(trimmed, StringComparison.OrdinalIgnoreCase));

            if (match is not null)
                return match;

            throw new ArgumentException($"Unknown status '{status}'.", nameof(status));
        }
    }
}