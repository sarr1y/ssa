namespace RecipesApi.DTOs;

public record RecipeDto(
    int Id,
    string Title,
    int CookingTimeMinutes,
    int Servings,
    string Ingredients,
    string Steps,
    int CuisineId,
    string CuisineName,
    string? ImageUrl,
    DateTime CreatedAt,
    int AuthorId,
    string AuthorUsername
);

public record RecipeCreateDto(
    string Title,
    int CookingTimeMinutes,
    int Servings,
    string Ingredients,
    string Steps,
    int CuisineId
);

public record RecipeUpdateDto(
    string Title,
    int CookingTimeMinutes,
    int Servings,
    string Ingredients,
    string Steps,
    int CuisineId
);

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Total,
    int Page,
    int PageSize,
    int TotalPages
);

public record ImageUploadResultDto(
    string ImageUrl,
    string Message
);
