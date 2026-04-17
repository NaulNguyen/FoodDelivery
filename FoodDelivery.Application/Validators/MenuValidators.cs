using FluentValidation;
using FoodDelivery.Application.DTOs;

namespace FoodDelivery.Application.Validators;

public class CreateMenuItemRequestValidator : AbstractValidator<CreateMenuItemRequest>
{
    public CreateMenuItemRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên món ăn không được để trống.")
            .MaximumLength(150).WithMessage("Tên món ăn không vượt quá 150 ký tự.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Giá món ăn phải lớn hơn hoặc bằng 0.");
    }
}

public class UpdateMenuItemRequestValidator : AbstractValidator<UpdateMenuItemRequest>
{
    public UpdateMenuItemRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên món ăn không được để trống.")
            .MaximumLength(150).WithMessage("Tên món ăn không vượt quá 150 ký tự.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Giá món ăn phải lớn hơn hoặc bằng 0.");
    }
}
