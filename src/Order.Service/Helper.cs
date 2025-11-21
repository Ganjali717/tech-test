using System;
using System.Collections.Generic;

namespace Order.Service;

public class Helper
{
    private static readonly Dictionary<string, string> StatusAliases =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["created"] = "Created",
            ["inprogress"] = "In Progress",
            ["in_progress"] = "In Progress",
            ["failed"] = "Failed",
            ["completed"] = "Completed"
        };

    public static string NormalizeStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status is required.", nameof(status));

        var key = status.Replace(" ", string.Empty);

        if (StatusAliases.TryGetValue(key, out var canonical))
            return canonical;
        
        return status;
    }

}