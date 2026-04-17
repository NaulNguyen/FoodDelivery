using FluentValidation;
using FoodDelivery.Application.DTOs;

namespace FoodDelivery.Application.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.RestaurantId)
            .NotEmpty().WithMessage("ID cửa hàng không được để trống.");

        RuleFor(x => x.Items)
            .NotNull().WithMessage("Giỏ hàng không được null.")
            .NotEmpty().WithMessage("Giỏ hàng không được để trống.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.MenuItemId)
                .NotEmpty().WithMessage("ID món ăn không được để trống.");
            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Số lượng món ăn phải lớn hơn 0.");
        });
    }
}

public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Trạng thái không được để trống.");
    }
}
