using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipesApi.Data;
using RecipesApi.DTOs;
using RecipesApi.Models;
using RecipesApi.Services;

namespace RecipesApi.Endpoints;

public static class RecipeEndpoints
{
    public static RouteGroupBuilder MapRecipeEndpoints(this RouteGroupBuilder group)
    {
        // 1. GET /api/recipes — Список рецептов с пагинацией, фильтрами и сортировкой (доступен всем)
        group.MapGet("/", async (
            AppDbContext context,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 6,
            [FromQuery] int? cuisineId = null,
            [FromQuery] int? maxTime = null,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = "newest") =>
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 6;
            if (pageSize > 50) pageSize = 50;

            var query = context.Recipes
                .Include(r => r.Cuisine)
                .Include(r => r.Author)
                .AsNoTracking()
                .AsQueryable();

            // Фильтр по кухне
            if (cuisineId.HasValue && cuisineId.Value > 0)
            {
                query = query.Where(r => r.CuisineId == cuisineId.Value);
            }

            // Фильтр по времени готовки (до N минут)
            if (maxTime.HasValue && maxTime.Value > 0)
            {
                query = query.Where(r => r.CookingTimeMinutes <= maxTime.Value);
            }

            // Поиск по ключевому слову
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(r => r.Title.ToLower().Contains(term) || r.Ingredients.ToLower().Contains(term));
            }

            // Сортировка (2 вида сортировки: по времени и по дате)
            query = (sortBy?.ToLower()) switch
            {
                "time_asc" => query.OrderBy(r => r.CookingTimeMinutes).ThenByDescending(r => r.CreatedAt),
                "time_desc" => query.OrderByDescending(r => r.CookingTimeMinutes).ThenByDescending(r => r.CreatedAt),
                "oldest" => query.OrderBy(r => r.CreatedAt),
                "newest" or _ => query.OrderByDescending(r => r.CreatedAt)
            };

            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RecipeDto(
                    r.Id,
                    r.Title,
                    r.CookingTimeMinutes,
                    r.Servings,
                    r.Ingredients,
                    r.Steps,
                    r.CuisineId,
                    r.Cuisine != null ? r.Cuisine.Name : string.Empty,
                    r.ImageUrl,
                    r.CreatedAt,
                    r.AuthorId,
                    r.Author != null ? r.Author.Username : "Аноним"
                ))
                .ToListAsync();

            return Results.Ok(new PagedResult<RecipeDto>(items, total, page, pageSize, totalPages));
        })
        .WithName("GetRecipes")
        .WithSummary("Получить список рецептов с пагинацией и фильтрами");

        // 2. GET /api/recipes/{id} — Получить рецепт по ID (доступен всем)
        group.MapGet("/{id:int}", async (int id, AppDbContext context) =>
        {
            var recipe = await context.Recipes
                .Include(r => r.Cuisine)
                .Include(r => r.Author)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Рецепт не найден",
                    Detail = $"Рецепт с идентификатором {id} не существует."
                });
            }

            var dto = new RecipeDto(
                recipe.Id,
                recipe.Title,
                recipe.CookingTimeMinutes,
                recipe.Servings,
                recipe.Ingredients,
                recipe.Steps,
                recipe.CuisineId,
                recipe.Cuisine != null ? recipe.Cuisine.Name : string.Empty,
                recipe.ImageUrl,
                recipe.CreatedAt,
                recipe.AuthorId,
                recipe.Author != null ? recipe.Author.Username : "Аноним"
            );

            return Results.Ok(dto);
        })
        .WithName("GetRecipeById")
        .WithSummary("Получить рецепт по идентификатору");

        // 3. POST /api/recipes — Создать рецепт (только для авторизованных, иначе 401)
        group.MapPost("/", async (
            RecipeCreateDto dto,
            IValidator<RecipeCreateDto> validator,
            ClaimsPrincipal user,
            AppDbContext context) =>
        {
            // Валидация через FluentValidation
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            // Получение ID автора из токена
            var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var currentUserId))
            {
                return Results.Unauthorized();
            }

            // Проверка кухни
            var cuisine = await context.Cuisines.FindAsync(dto.CuisineId);
            if (cuisine == null)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    { nameof(dto.CuisineId), new[] { $"Кухня с ID {dto.CuisineId} не найдена в справочнике." } }
                });
            }

            var author = await context.Users.FindAsync(currentUserId);
            if (author == null)
            {
                return Results.Unauthorized();
            }

            var recipe = new Recipe
            {
                Title = dto.Title.Trim(),
                CookingTimeMinutes = dto.CookingTimeMinutes,
                Servings = dto.Servings,
                Ingredients = dto.Ingredients.Trim(),
                Steps = dto.Steps.Trim(),
                CuisineId = dto.CuisineId,
                AuthorId = currentUserId,
                CreatedAt = DateTime.UtcNow
            };

            context.Recipes.Add(recipe);
            await context.SaveChangesAsync();

            var createdDto = new RecipeDto(
                recipe.Id,
                recipe.Title,
                recipe.CookingTimeMinutes,
                recipe.Servings,
                recipe.Ingredients,
                recipe.Steps,
                recipe.CuisineId,
                cuisine.Name,
                recipe.ImageUrl,
                recipe.CreatedAt,
                recipe.AuthorId,
                author.Username
            );

            return Results.Created($"/api/recipes/{recipe.Id}", createdDto);
        })
        .RequireAuthorization()
        .WithName("CreateRecipe")
        .WithSummary("Создать новый рецепт (только авторизованный пользователь)");

        // 4. PUT /api/recipes/{id} — Редактировать рецепт (только автор, иначе 403)
        group.MapPut("/{id:int}", async (
            int id,
            RecipeUpdateDto dto,
            IValidator<RecipeUpdateDto> validator,
            ClaimsPrincipal user,
            AppDbContext context) =>
        {
            var recipe = await context.Recipes
                .Include(r => r.Cuisine)
                .Include(r => r.Author)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Рецепт не найден",
                    Detail = $"Рецепт с ID {id} не найден."
                });
            }

            var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var currentUserId))
            {
                return Results.Unauthorized();
            }

            // Проверка авторства: менять может только автор, иначе 403
            if (recipe.AuthorId != currentUserId)
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            // Валидация через FluentValidation
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            // Проверка кухни
            var cuisine = await context.Cuisines.FindAsync(dto.CuisineId);
            if (cuisine == null)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    { nameof(dto.CuisineId), new[] { $"Кухня с ID {dto.CuisineId} не найдена в справочнике." } }
                });
            }

            recipe.Title = dto.Title.Trim();
            recipe.CookingTimeMinutes = dto.CookingTimeMinutes;
            recipe.Servings = dto.Servings;
            recipe.Ingredients = dto.Ingredients.Trim();
            recipe.Steps = dto.Steps.Trim();
            recipe.CuisineId = dto.CuisineId;

            await context.SaveChangesAsync();

            var updatedDto = new RecipeDto(
                recipe.Id,
                recipe.Title,
                recipe.CookingTimeMinutes,
                recipe.Servings,
                recipe.Ingredients,
                recipe.Steps,
                recipe.CuisineId,
                cuisine.Name,
                recipe.ImageUrl,
                recipe.CreatedAt,
                recipe.AuthorId,
                recipe.Author?.Username ?? "Аноним"
            );

            return Results.Ok(updatedDto);
        })
        .RequireAuthorization()
        .WithName("UpdateRecipe")
        .WithSummary("Редактировать рецепт (только автор записи)");

        // 5. DELETE /api/recipes/{id} — Удалить рецепт (только автор, иначе 403)
        group.MapDelete("/{id:int}", async (
            int id,
            ClaimsPrincipal user,
            AppDbContext context,
            IFileStorageService fileStorage) =>
        {
            var recipe = await context.Recipes.FirstOrDefaultAsync(r => r.Id == id);
            if (recipe == null)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Рецепт не найден",
                    Detail = $"Рецепт с ID {id} не найден."
                });
            }

            var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var currentUserId))
            {
                return Results.Unauthorized();
            }

            // Проверка авторства: удалять может только автор, иначе 403
            if (recipe.AuthorId != currentUserId)
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            // Удаляем фото с диска, если есть
            if (!string.IsNullOrEmpty(recipe.ImageUrl))
            {
                fileStorage.DeleteFile(recipe.ImageUrl);
            }

            context.Recipes.Remove(recipe);
            await context.SaveChangesAsync();

            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("DeleteRecipe")
        .WithSummary("Удалить рецепт (только автор записи)");

        // 6. POST /api/recipes/{id}/image — Загрузка одного файла к записи (только автор, до 5 МБ, jpg/png/webp, имя Guid)
        group.MapPost("/{id:int}/image", async (
            int id,
            IFormFile file,
            ClaimsPrincipal user,
            AppDbContext context,
            IFileStorageService fileStorage) =>
        {
            var recipe = await context.Recipes.FirstOrDefaultAsync(r => r.Id == id);
            if (recipe == null)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Рецепт не найден",
                    Detail = $"Рецепт с ID {id} не найден."
                });
            }

            var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var currentUserId))
            {
                return Results.Unauthorized();
            }

            // Загрузка только автором, иначе 403
            if (recipe.AuthorId != currentUserId)
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            try
            {
                var newImageUrl = await fileStorage.SaveFileAsync(file, "recipes");

                // Удаляем старое фото, если оно было загружено ранее
                if (!string.IsNullOrEmpty(recipe.ImageUrl) && !recipe.ImageUrl.Contains("placeholder"))
                {
                    fileStorage.DeleteFile(recipe.ImageUrl);
                }

                recipe.ImageUrl = newImageUrl;
                await context.SaveChangesAsync();

                return Results.Ok(new ImageUploadResultDto(newImageUrl, "Изображение успешно загружено."));
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    { "File", new[] { ex.Message } }
                });
            }
        })
        .DisableAntiforgery()
        .RequireAuthorization()
        .WithName("UploadRecipeImage")
        .WithSummary("Загрузить изображение к рецепту (только автор)");

        return group;
    }
}
