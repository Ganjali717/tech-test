using System.Collections.Generic;

namespace Order.Service.Status
{
    public static class OrderStatuses
    {
        public const string Created = "Created";
        public const string InProgress = "In Progress";
        public const string Failed = "Failed";
        public const string Completed = "Completed";

        public static IReadOnlyCollection<string> All { get; } =
            new[] { Created, InProgress, Failed, Completed };
    }
}