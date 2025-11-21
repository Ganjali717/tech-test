using FluentValidation;
using Order.Model.Requests;

namespace OrderService.WebAPI.Validation;
public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.");
    }
}