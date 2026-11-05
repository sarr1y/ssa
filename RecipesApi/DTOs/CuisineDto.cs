namespace RecipesApi.DTOs;

public record CuisineDto(
    int Id,
    string Name,
    int RecipeCount = 0
);
