using System.Security.Claims;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RecipesApi.Data;
using RecipesApi.DTOs;
using RecipesApi.Models;
using RecipesApi.Services;

namespace RecipesApi.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
    {
        // Регистрация нового пользователя
        group.MapPost("/register", async (
            RegisterDto dto,
            IValidator<RegisterDto> validator,
            AppDbContext context,
            IPasswordHasherService passwordHasher,
            IJwtService jwtService) =>
        {
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var username = dto.Username.Trim();
            var existingUser = await context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

            if (existingUser != null)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    { nameof(dto.Username), new[] { "Пользователь с таким именем уже существует." } }
                });
            }

            var user = new User
            {
                Username = username,
                PasswordHash = passwordHasher.HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var token = jwtService.GenerateToken(user);
            return Results.Created($"/api/auth/me", new AuthResponseDto(token, user.Id, user.Username));
        })
        .WithName("Register")
        .WithSummary("Регистрация нового пользователя");

        // Вход в систему (получение JWT токена)
        group.MapPost("/login", async (
            LoginDto dto,
            IValidator<LoginDto> validator,
            AppDbContext context,
            IPasswordHasherService passwordHasher,
            IJwtService jwtService) =>
        {
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var username = dto.Username.Trim();
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

            if (user == null || !passwordHasher.VerifyPassword(user.PasswordHash, dto.Password))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    { "Credentials", new[] { "Неверное имя пользователя или пароль." } }
                });
            }

            var token = jwtService.GenerateToken(user);
            return Results.Ok(new AuthResponseDto(token, user.Id, user.Username));
        })
        .WithName("Login")
        .WithSummary("Вход в систему");

        // Получение данных текущего пользователя
        group.MapGet("/me", (ClaimsPrincipal user) =>
        {
            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var name = user.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new { id = int.Parse(id), username = name });
        })
        .RequireAuthorization()
        .WithName("GetCurrentUser")
        .WithSummary("Получить данные вошедшего пользователя");

        return group;
    }
}
