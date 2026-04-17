using FluentValidation;
using FoodDelivery.Application.DTOs;

namespace FoodDelivery.Application.Validators;

public class CreateRestaurantRequestValidator : AbstractValidator<CreateRestaurantRequest>
{
    public CreateRestaurantRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên cửa hàng không được để trống.")
            .MaximumLength(100).WithMessage("Tên cửa hàng không được vượt quá 100 ký tự.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Địa chỉ không được để trống.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Số điện thoại không được để trống.")
            .Matches(@"^[0-9]{10,11}$").WithMessage("Số điện thoại không hợp lệ (10-11 số).");
    }
}

public class UpdateRestaurantRequestValidator : AbstractValidator<UpdateRestaurantRequest>
{
    public UpdateRestaurantRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên cửa hàng không được để trống.")
            .MaximumLength(100).WithMessage("Tên cửa hàng không được vượt quá 100 ký tự.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Địa chỉ không được để trống.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Số điện thoại không được để trống.")
            .Matches(@"^[0-9]{10,11}$").WithMessage("Số điện thoại không hợp lệ (10-11 số).");
    }
}
