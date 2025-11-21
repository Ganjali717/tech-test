using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Order.Service.Exceptions;

namespace OrderService.WebAPI.Middleware
{
    public sealed class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (OrderNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order not found");
                await WriteProblemAsync(
                    context,
                    StatusCodes.Status404NotFound,
                    title: "Order not found",
                    detail: ex.Message);
            }
            catch (InvalidOrderStatusTransitionException ex)
            {
                _logger.LogWarning(ex, "Invalid order status transition");
                await WriteProblemAsync(
                    context,
                    StatusCodes.Status409Conflict,
                    title: "Invalid order status transition",
                    detail: ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request");
                await WriteProblemAsync(
                    context,
                    StatusCodes.Status400BadRequest,
                    title: "Invalid request",
                    detail: ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                await WriteProblemAsync(
                    context,
                    StatusCodes.Status500InternalServerError,
                    title: "Internal server error",
                    detail: "An unexpected error occurred. Please try again later.");
            }
        }

        private static async Task WriteProblemAsync(
            HttpContext context,
            int statusCode,
            string title,
            string detail)
        {
            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var json = JsonSerializer.Serialize(problem);
            await context.Response.WriteAsync(json);
        }
    }
}
