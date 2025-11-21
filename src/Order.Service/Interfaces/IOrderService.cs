using Order.Model.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Service.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderSummary>> GetOrdersAsync();
        Task<OrderDetail> GetOrderByIdAsync(Guid orderId);
        Task<IEnumerable<OrderSummary>> GetOrdersByStatusAsync(string status);
        Task<OrderDetail> UpdateOrderStatusAsync(Guid orderId, string newStatus);
    }
}
