using Order.Data.Repositories;
using Order.Model.DTOs;
using Order.Service.Exceptions;
using Order.Service.Interfaces;
using Order.Service.Status;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<OrderDetail> UpdateOrderStatusAsync(Guid orderId, string newStatus)
        {
            if (string.IsNullOrWhiteSpace(newStatus))
                throw new ArgumentException("Status is required.", nameof(newStatus));

            var current = await _orderRepository.GetOrderByIdAsync(orderId);
            if (current == null)
                throw new OrderNotFoundException(orderId);

            var currentStatus = current.StatusName;
            var targetStatus = newStatus.Trim();

            OrderStatusTransitions.Validate(currentStatus, targetStatus);

            await _orderRepository.UpdateOrderStatusAsync(orderId, targetStatus);

            return await _orderRepository.GetOrderByIdAsync(orderId);
        }

    }
}
