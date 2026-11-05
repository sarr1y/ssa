using Microsoft.EntityFrameworkCore;
using RecipesApi.Data;
using RecipesApi.DTOs;

namespace RecipesApi.Endpoints;

public static class CuisineEndpoints
{
    public static RouteGroupBuilder MapCuisineEndpoints(this RouteGroupBuilder group)
    {
        // GET /api/cuisines — Справочник кухонь мира с подсчётом рецептов
        group.MapGet("/", async (AppDbContext context) =>
        {
            var cuisines = await context.Cuisines
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new CuisineDto(
                    c.Id,
                    c.Name,
                    c.Recipes.Count
                ))
                .ToListAsync();

            return Results.Ok(cuisines);
        })
        .WithName("GetCuisines")
        .WithSummary("Получить список национальных кухонь");

        return group;
    }
}
