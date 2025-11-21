using Order.Data.Repositories;
using Order.Model;
using Order.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public Task<IEnumerable<OrderSummary>> GetOrdersAsync()
            => _orderRepository.GetOrdersAsync();

        public Task<OrderDetail> GetOrderByIdAsync(Guid orderId)
            => _orderRepository.GetOrderByIdAsync(orderId);

        public Task<IEnumerable<OrderSummary>> GetOrdersByStatusAsync(string status)
            => _orderRepository.GetOrdersByStatusAsync(status);
    }
}
