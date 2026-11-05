namespace RecipesApi.Models;

public class Recipe
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int CookingTimeMinutes { get; set; }
    public int Servings { get; set; }
    public string Ingredients { get; set; } = string.Empty;
    public string Steps { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int CuisineId { get; set; }
    public Cuisine? Cuisine { get; set; }

    public int AuthorId { get; set; }
    public User? Author { get; set; }
}
