using FluentValidation;
using RecipesApi.DTOs;

namespace RecipesApi.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Имя пользователя обязательно.")
            .MinimumLength(3).WithMessage("Имя пользователя должно содержать не менее 3 символов.")
            .MaximumLength(50).WithMessage("Имя пользователя не должно превышать 50 символов.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен.")
            .MinimumLength(6).WithMessage("Пароль должен содержать не менее 6 символов.");
    }
}

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Введите имя пользователя.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Введите пароль.");
    }
}
