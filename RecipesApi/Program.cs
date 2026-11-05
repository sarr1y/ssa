using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RecipesApi.Data;
using RecipesApi.Endpoints;
using RecipesApi.Services;
using RecipesApi.Validators;

var builder = WebApplication.CreateBuilder(args);

// Привязка ко всем интерфейсам на порту 5000 (localhost, 127.0.0.1, любые хосты)
builder.WebHost.UseUrls("http://*:5000");

// 1. EF Core с SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=recipes.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// 2. Сервисы приложения
builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

// 3. Регистрация FluentValidation валидаторов
builder.Services.AddValidatorsFromAssemblyContaining<RecipeCreateDtoValidator>();

// 4. Настройка аутентификации JWT
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? "UltraSuperSecretKeyForRecipesWebApiTokenSigning2026!#";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "RecipesApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "RecipesClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 5. Открытый CORS для взаимодействия с Angular и Swagger
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 6. Swagger с поддержкой JWT Bearer авторизации
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Recipes Minimal API (.NET 10)",
        Version = "v1",
        Description = "Кулинарная книга рецептов: Minimal API + EF Core (SQLite) + FluentValidation + JWT"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введите JWT токен (только сам токен, без префикса 'Bearer ')."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Страница подробных ошибок для разработки
app.UseDeveloperExceptionPage();

// Применение миграций базы данных при старте
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbInitializer.Initialize(context);
}

// Создание папки uploads/recipes для сохранения фото блюд
var webRoot = app.Environment.WebRootPath;
if (string.IsNullOrEmpty(webRoot))
{
    webRoot = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
}
var uploadsDir = Path.Combine(webRoot, "uploads", "recipes");
if (!Directory.Exists(uploadsDir))
{
    Directory.CreateDirectory(uploadsDir);
}

// Swagger включён на постоянной основе (и по /swagger, и по корню /)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Recipes Minimal API v1");
    c.RoutePrefix = "swagger";
});
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Recipes Minimal API v1");
    c.RoutePrefix = string.Empty;
});

// Статические файлы (раздача загруженных изображений)
app.UseStaticFiles();

// CORS
app.UseCors("AllowAll");

// Аутентификация и авторизация
app.UseAuthentication();
app.UseAuthorization();

// Статусный эндпоинт для быстрой проверки доступности API
app.MapGet("/api", () => Results.Ok(new
{
    status = "Online",
    service = "Recipes Minimal API (.NET 10)",
    swagger = "http://localhost:5000/swagger",
    recipes = "http://localhost:5000/api/recipes",
    cuisines = "http://localhost:5000/api/cuisines",
    auth = "http://localhost:5000/api/auth",
    frontend = "http://localhost:4200"
}));

// Регистрация групп Minimal API
app.MapGroup("/api/recipes").MapRecipeEndpoints().WithTags("Recipes");
app.MapGroup("/api/auth").MapAuthEndpoints().WithTags("Authentication");
app.MapGroup("/api/cuisines").MapCuisineEndpoints().WithTags("Cuisines");

Console.WriteLine("==================================================");
Console.WriteLine("  Recipes Minimal API (.NET 10) успешно запущен!");
Console.WriteLine("  Главная / Swagger: http://localhost:5000");
Console.WriteLine("  Swagger UI:        http://localhost:5000/swagger");
Console.WriteLine("  Проверка API:      http://localhost:5000/api");
Console.WriteLine("  Рецепты:           http://localhost:5000/api/recipes");
Console.WriteLine("  Справочник кухонь: http://localhost:5000/api/cuisines");
Console.WriteLine("==================================================");

app.Run();
