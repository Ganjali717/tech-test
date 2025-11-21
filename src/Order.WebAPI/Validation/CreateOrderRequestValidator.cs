using System.Linq;
using FluentValidation;
using Order.Model.Requests;

namespace OrderService.WebAPI.Validation;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.Items)
            .Must(items =>
            {
                return items
                    .GroupBy(i => new { i.ProductId })
                    .All(g => g.Count() == 1);
            })
            .WithMessage("Each productId must appear only once in items.");

        RuleFor(x => x.ResellerId)
            .NotEmpty().WithMessage("ResellerId is required.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one order item is required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty().WithMessage("ProductId is required.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        });
    }
}