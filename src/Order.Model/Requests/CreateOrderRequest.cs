using System;
using System.Collections.Generic;

namespace Order.Model.Requests
{
    public class CreateOrderRequest
    {
        public Guid ResellerId { get; set; }
        public Guid CustomerId { get; set; }
        public IList<CreateOrderItemRequest> Items { get; set; } = new List<CreateOrderItemRequest>();
    }

    public class CreateOrderItemRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}