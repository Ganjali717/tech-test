using Microsoft.EntityFrameworkCore;
using Order.Data.Context;
using Order.Model;
using Order.Model.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Order.Model.Requests;

namespace Order.Data.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderContext _orderContext;

        public OrderRepository(OrderContext orderContext)
        {
            _orderContext = orderContext;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersAsync()
        {
            var orders = await _orderContext.Order
                .Include(x => x.Items)
                .Include(x => x.Status)
                .Select(x => new OrderSummary
                {
                    Id = new Guid(x.Id),
                    ResellerId = new Guid(x.ResellerId),
                    CustomerId = new Guid(x.CustomerId),
                    StatusId = new Guid(x.StatusId),
                    StatusName = x.Status.Name,
                    ItemCount = x.Items.Count,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    CreatedDate = x.CreatedDate
                })
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return orders;
        }

        public async Task<OrderDetail> GetOrderByIdAsync(Guid orderId)
        {
            var orderIdBytes = orderId.ToByteArray();

            var order = await _orderContext.Order
                .Where(x => _orderContext.Database.IsInMemory() ? x.Id.SequenceEqual(orderIdBytes) : x.Id == orderIdBytes)
                .Select(x => new OrderDetail
                {
                    Id = new Guid(x.Id),
                    ResellerId = new Guid(x.ResellerId),
                    CustomerId = new Guid(x.CustomerId),
                    StatusId = new Guid(x.StatusId),
                    StatusName = x.Status.Name,
                    CreatedDate = x.CreatedDate,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    Items = x.Items.Select(i => new OrderItem
                    {
                        Id = new Guid(i.Id),
                        OrderId = new Guid(i.OrderId),
                        ServiceId = new Guid(i.ServiceId),
                        ServiceName = i.Service.Name,
                        ProductId = new Guid(i.ProductId),
                        ProductName = i.Product.Name,
                        UnitCost = i.Product.UnitCost,
                        UnitPrice = i.Product.UnitPrice,
                        TotalCost = i.Product.UnitCost * i.Quantity.Value,
                        TotalPrice = i.Product.UnitPrice * i.Quantity.Value,
                        Quantity = i.Quantity.Value
                    })
                }).SingleOrDefaultAsync();
            
            return order;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersByStatusAsync(string statusName)
        {
            statusName = statusName?.Trim();

            var query = _orderContext.Order
                .Include(o => o.Status)
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(statusName))
            {
                query = query.Where(o => o.Status.Name == statusName);
            }

            var orders = await query
                .Select(o => new OrderSummary
                {
                    Id = new Guid(o.Id),
                    ResellerId = new Guid(o.ResellerId),
                    CustomerId = new Guid(o.CustomerId),
                    StatusId = new Guid(o.StatusId),
                    StatusName = o.Status.Name,
                    ItemCount = o.Items.Count,
                    TotalCost = o.Items.Sum(i => i.Product.UnitCost * i.Quantity.Value),
                    TotalPrice = o.Items.Sum(i => i.Product.UnitPrice * i.Quantity.Value),
                    CreatedDate = o.CreatedDate
                })
                .ToListAsync()
                .ConfigureAwait(false);

            return orders;
        }

        public async Task UpdateOrderStatusAsync(Guid orderId, string newStatus)
        {
            var orderBytes = orderId.ToByteArray();

            var order = await _orderContext.Order
                .Include(o => o.Status)
                .SingleOrDefaultAsync(o => o.Id == orderBytes);

            if (order == null)
                return;

            var status = await _orderContext.OrderStatus
                .SingleOrDefaultAsync(s => s.Name == newStatus);

            if (status == null)
                throw new InvalidOperationException($"Unknown status '{newStatus}'.");

            order.StatusId = status.Id;
            order.Status = status;

            await _orderContext.SaveChangesAsync();
        }

        public async Task<OrderDetail> CreateOrderAsync(CreateOrderRequest request)
        {
            var createdStatus = await _orderContext.OrderStatus
                .SingleAsync(s => s.Name == "Created")
                .ConfigureAwait(false);

            var now = DateTime.UtcNow;
            var orderId = Guid.NewGuid();

            var orderEntity = new Entities.Order
            {
                Id = orderId.ToByteArray(),
                ResellerId = request.ResellerId.ToByteArray(),
                CustomerId = request.CustomerId.ToByteArray(),
                StatusId = createdStatus.Id,
                Status = createdStatus,
                CreatedDate = now
            };

            foreach (var item in request.Items)
            {
                var product = await _orderContext.OrderProduct
                    .SingleAsync(p => p.Id == item.ProductId.ToByteArray())
                    .ConfigureAwait(false);

                var orderItem = new Entities.OrderItem
                {
                    Id = Guid.NewGuid().ToByteArray(),
                    Order = orderEntity,
                    OrderId = orderEntity.Id,
                    Product = product,
                    ProductId = product.Id,
                    Service = product.Service,
                    ServiceId = product.ServiceId,
                    Quantity = item.Quantity
                };

                orderEntity.Items.Add(orderItem);
            }

            _orderContext.Order.Add(orderEntity);
            await _orderContext.SaveChangesAsync().ConfigureAwait(false);
            
            return await GetOrderByIdAsync(orderId).ConfigureAwait(false);
        }

        public async Task<IEnumerable<MonthlyProfit>> GetMonthlyProfitAsync()
        {
            var items = await _orderContext.OrderItem
                .Include(i => i.Order)
                .Include(i => i.Product)
                .Select(i => new
                {
                    i.Order.CreatedDate,
                    Profit = (i.Product.UnitPrice - i.Product.UnitCost) * i.Quantity.Value
                })
                .ToListAsync()
                .ConfigureAwait(false);
            
            var result = items
                .GroupBy(x => new { x.CreatedDate.Year, x.CreatedDate.Month })
                .Select(g => new MonthlyProfit
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalProfit = g.Sum(x => x.Profit)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            return result;
        }


    }
}
