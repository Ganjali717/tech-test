using Microsoft.Extensions.Logging;
using Order.Data.Repositories;
using Order.Model;
using Order.Model.DTOs;
using Order.Model.Requests;
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
        private readonly ILogger<OrderService> _logger;

        public OrderService(IOrderRepository orderRepository, ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersAsync()
            => await _orderRepository.GetOrdersAsync();

        public async Task<OrderDetail> GetOrderByIdAsync(Guid orderId)
            => await _orderRepository.GetOrderByIdAsync(orderId);

        public async Task<IEnumerable<OrderSummary>> GetOrdersByStatusAsync(string status)
            => await _orderRepository.GetOrdersByStatusAsync(status);

        public async Task<OrderDetail> UpdateOrderStatusAsync(Guid orderId, string newStatus)
        {
            var targetStatus = Helper.NormalizeStatus(newStatus);

            var current = await _orderRepository.GetOrderByIdAsync(orderId)
                          ?? throw new OrderNotFoundException(orderId);

            var currentStatus = current.StatusName;

            _logger.LogInformation(
                "Attempting to change order {OrderId} status {OldStatus} -> {NewStatus}",
                orderId, currentStatus, targetStatus);

            OrderStatusTransitions.Validate(currentStatus, targetStatus);

            await _orderRepository.UpdateOrderStatusAsync(orderId, targetStatus);

            var updated = await _orderRepository.GetOrderByIdAsync(orderId);

            _logger.LogInformation(
                "Order {OrderId} status successfully changed to {NewStatus}",
                orderId, targetStatus);

            return updated;
        }

        public async Task<OrderDetail> CreateOrderAsync(CreateOrderRequest request)
        {
            _logger.LogInformation(
                "Creating order for Reseller {ResellerId}, Customer {CustomerId} with {ItemCount} items",
                request.ResellerId, request.CustomerId, request.Items.Count);

            var created = await _orderRepository.CreateOrderAsync(request);

            _logger.LogInformation(
                "Order {OrderId} created with status {Status}",
                created.Id, created.StatusName);

            return created;
        }

        public async Task<IEnumerable<MonthlyProfit>> GetMonthlyProfitAsync()
        {
            _logger.LogInformation("Calculating monthly profit for orders");
            return await _orderRepository.GetMonthlyProfitAsync();
        }

    }
}
