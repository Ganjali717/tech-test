using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.Model.DTOs;
using Order.Model.Requests;
using Order.Service.Exceptions;
using Order.Service.Interfaces;
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

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
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
        public async Task<IActionResult> UpdateStatus(Guid orderId, [FromBody] UpdateOrderStatusRequest request)
        {
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
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
