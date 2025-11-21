using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.Model.DTOs;
using Order.Model.Requests;
using Order.Service.Interfaces;

namespace OrderService.WebAPI.Controllers
{
    [ApiController]
    [Route("orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IValidator<CreateOrderRequest> _createOrderValidator;
        private readonly IValidator<UpdateOrderStatusRequest> _updateStatusValidator;

        public OrderController(
            IOrderService orderService,
            IValidator<CreateOrderRequest> createOrderValidator,
            IValidator<UpdateOrderStatusRequest> updateStatusValidator)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _createOrderValidator = createOrderValidator ?? throw new ArgumentNullException(nameof(createOrderValidator));
            _updateStatusValidator = updateStatusValidator ?? throw new ArgumentNullException(nameof(updateStatusValidator));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderSummary>>> GetOrders([FromQuery] string? status)
        {
            if (!string.IsNullOrWhiteSpace(status))
            {
                var filtered = await _orderService.GetOrdersByStatusAsync(status);
                return Ok(filtered);
            }

            var orders = await _orderService.GetOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("{orderId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<OrderDetail>> GetOrderById(Guid orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            return Ok(order);
        }

        [HttpPut("{orderId:guid}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<OrderDetail>> UpdateOrderStatus(
            Guid orderId,
            [FromBody] UpdateOrderStatusRequest request)
        {
            var validationResult = await _updateStatusValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }

                return ValidationProblem(ModelState);
            }

            var updated = await _orderService.UpdateOrderStatusAsync(orderId, request.Status);
            return Ok(updated);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<OrderDetail>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var validationResult = await _createOrderValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }

                return ValidationProblem(ModelState);
            }

            var created = await _orderService.CreateOrderAsync(request);
            return CreatedAtAction(nameof(GetOrderById), new { orderId = created.Id }, created);
        }

        [HttpGet("profit-by-month")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProfitByMonth()
        {
            var result = await _orderService.GetMonthlyProfitAsync();
            return Ok(result);
        }
    }
}
