using Order.Data.Repositories;
using Order.Model.DTOs;
using Order.Service.Exceptions;
using Order.Service.Interfaces;
using Order.Service.Status;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Order.Model;
using Order.Model.Requests;

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
            var current = await _orderRepository.GetOrderByIdAsync(orderId)
                          ?? throw new OrderNotFoundException(orderId);

            var currentStatus = current.StatusName;
            var targetStatus = Helper.NormalizeStatus(newStatus);
            OrderStatusTransitions.Validate(currentStatus, targetStatus);

            await _orderRepository.UpdateOrderStatusAsync(orderId, targetStatus);
            return await _orderRepository.GetOrderByIdAsync(orderId);
        }

        public async Task<OrderDetail> CreateOrderAsync(CreateOrderRequest request)
            => await _orderRepository.CreateOrderAsync(request);

        public Task<IEnumerable<MonthlyProfit>> GetMonthlyProfitAsync()
            => _orderRepository.GetMonthlyProfitAsync();

    }
}
