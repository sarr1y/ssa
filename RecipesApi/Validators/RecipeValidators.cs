using FluentValidation;
using RecipesApi.DTOs;

namespace RecipesApi.Validators;

public class RecipeCreateDtoValidator : AbstractValidator<RecipeCreateDto>
{
    public RecipeCreateDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название рецепта обязательно.")
            .MaximumLength(200).WithMessage("Название не может быть длиннее 200 символов.");

        RuleFor(x => x.CookingTimeMinutes)
            .GreaterThan(0).WithMessage("Время приготовления должно быть больше 0 минут.");

        RuleFor(x => x.Servings)
            .InclusiveBetween(1, 100).WithMessage("Количество порций должно быть от 1 до 100.");

        RuleFor(x => x.Ingredients)
            .NotEmpty().WithMessage("Список ингредиентов обязателен.");

        RuleFor(x => x.Steps)
            .NotEmpty().WithMessage("Пошаговые инструкции обязательны.");

        RuleFor(x => x.CuisineId)
            .GreaterThan(0).WithMessage("Выберите кухню из справочника.");

        // Особое правило вашего варианта:
        // «Если время больше 240 минут, шаги не короче 200 символов.»
        When(x => x.CookingTimeMinutes > 240, () =>
        {
            RuleFor(x => x.Steps)
                .MinimumLength(200)
                .WithMessage("Особое правило: если время больше 240 минут, шаги не короче 200 символов.");
        });
    }
}

public class RecipeUpdateDtoValidator : AbstractValidator<RecipeUpdateDto>
{
    public RecipeUpdateDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название рецепта обязательно.")
            .MaximumLength(200).WithMessage("Название не может быть длиннее 200 символов.");

        RuleFor(x => x.CookingTimeMinutes)
            .GreaterThan(0).WithMessage("Время приготовления должно быть больше 0 минут.");

        RuleFor(x => x.Servings)
            .InclusiveBetween(1, 100).WithMessage("Количество порций должно быть от 1 до 100.");

        RuleFor(x => x.Ingredients)
            .NotEmpty().WithMessage("Список ингредиентов обязателен.");

        RuleFor(x => x.Steps)
            .NotEmpty().WithMessage("Пошаговые инструкции обязательны.");

        RuleFor(x => x.CuisineId)
            .GreaterThan(0).WithMessage("Выберите кухню из справочника.");

        // Особое правило вашего варианта:
        // «Если время больше 240 минут, шаги не короче 200 символов.»
        When(x => x.CookingTimeMinutes > 240, () =>
        {
            RuleFor(x => x.Steps)
                .MinimumLength(200)
                .WithMessage("Особое правило: если время больше 240 минут, шаги не короче 200 символов.");
        });
    }
}
