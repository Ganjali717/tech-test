using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.Model.DTOs;
using Order.Model.Requests;
using Order.Service.Exceptions;
using Order.Service.Interfaces;
using OrderService.WebAPI.Validation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            _orderService = orderService;
            _createOrderValidator = createOrderValidator;
            _updateStatusValidator = updateStatusValidator;
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get([FromQuery] string? status)
        {
            IEnumerable<OrderSummary> orders;

            if (string.IsNullOrWhiteSpace(status))
                orders = await _orderService.GetOrdersAsync();
            else
                orders = await _orderService.GetOrdersByStatusAsync(status);

            return Ok(orders);
        }


        [HttpGet("{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderById(Guid orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            return order is null ? NotFound() : Ok(order);
        }


        [HttpPut("{orderId:guid}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateStatus(Guid orderId, [FromBody] UpdateOrderStatusRequest request)
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

            try
            {
                var updated = await _orderService.UpdateOrderStatusAsync(orderId, request.Status);
                return Ok(updated);
            }
            catch (OrderNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOrderStatusTransitionException ex)
            {
                return Conflict(ex.Message);
            }
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
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
