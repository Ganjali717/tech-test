using Order.Model;
using Order.Model.DTOs;
using Order.Model.Requests;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Data.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<OrderSummary>> GetOrdersAsync();
        Task<OrderDetail> GetOrderByIdAsync(Guid orderId);
        Task<IEnumerable<OrderSummary>> GetOrdersByStatusAsync(string statusName);
        Task UpdateOrderStatusAsync(Guid orderId, string newStatus);
        Task<OrderDetail> CreateOrderAsync(CreateOrderRequest request);
        Task<IEnumerable<MonthlyProfit>> GetMonthlyProfitAsync();

    }
}
