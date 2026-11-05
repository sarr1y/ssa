using Microsoft.EntityFrameworkCore;
using RecipesApi.Models;
using RecipesApi.Services;

namespace RecipesApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<Cuisine> Cuisines => Set<Cuisine>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Конфигурация пользователей (User)
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.CreatedAt).IsRequired();
        });

        // 2. Конфигурация справочника (Cuisine)
        modelBuilder.Entity<Cuisine>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(c => c.Name).IsUnique();
        });

        // 3. Конфигурация рецепта (Recipe)
        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Title).IsRequired().HasMaxLength(200);
            entity.Property(r => r.Ingredients).IsRequired();
            entity.Property(r => r.Steps).IsRequired();
            entity.Property(r => r.CookingTimeMinutes).IsRequired();
            entity.Property(r => r.Servings).IsRequired();
            entity.Property(r => r.CreatedAt).IsRequired();

            entity.HasOne(r => r.Cuisine)
                  .WithMany(c => c.Recipes)
                  .HasForeignKey(r => r.CuisineId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Author)
                  .WithMany(u => u.Recipes)
                  .HasForeignKey(r => r.AuthorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Начальные данные (Seed data через HasData):

        // Тестовые пользователи: chef (пароль Chef123!) и testuser (пароль User123!)
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Username = "chef",
                PasswordHash = PasswordHasherService.DefaultChefHash,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new User
            {
                Id = 2,
                Username = "testuser",
                PasswordHash = PasswordHasherService.DefaultUserHash,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // Справочник кухонь мира
        modelBuilder.Entity<Cuisine>().HasData(
            new Cuisine { Id = 1, Name = "Итальянская" },
            new Cuisine { Id = 2, Name = "Грузинская" },
            new Cuisine { Id = 3, Name = "Узбекская" },
            new Cuisine { Id = 4, Name = "Французская" },
            new Cuisine { Id = 5, Name = "Азиатская" },
            new Cuisine { Id = 6, Name = "Русская" },
            new Cuisine { Id = 7, Name = "Мексиканская" }
        );

        // 7 начальных рецептов
        modelBuilder.Entity<Recipe>().HasData(
            new Recipe
            {
                Id = 1,
                Title = "Классическая паста Карбонара",
                CookingTimeMinutes = 25,
                Servings = 2,
                Ingredients = "• Спагетти — 200 г\n• Гуанчале или панчетта — 100 г\n• Яичные желтки — 3 шт.\n• Сыр Пармезан или Пекорино — 50 г\n• Свежемолотый чёрный перец — по вкусу\n• Морская соль — для варки",
                Steps = "1. Поставьте воду для пасты закипать, посолите.\n2. Нарежьте гуанчале полосками и обжарьте на сковороде на среднем огне до хрустящей корочки.\n3. В отдельной миске взбейте желтки с натертым сыром и перцем.\n4. Отварите спагетти до состояния al dente, переложите в сковороду с мясом, снимите с огня.\n5. Быстро влейте яично-сырную смесь и немного воды из-под пасты, энергично перемешивая до шелковистого соуса.",
                CuisineId = 1,
                AuthorId = 1,
                ImageUrl = "/uploads/recipes/carbonara.webp",
                CreatedAt = new DateTime(2026, 10, 20, 12, 0, 0, DateTimeKind.Utc)
            },
            new Recipe
            {
                Id = 2,
                Title = "Хачапури по-аджарски",
                CookingTimeMinutes = 45,
                Servings = 2,
                Ingredients = "• Мука пшеничная — 350 г\n• Дрожжи сухие — 5 г\n• Молоко тёплое — 200 мл\n• Сыр сулугуни — 300 г\n• Сыр имеретинский — 150 г\n• Яйца куриные — 3 шт.\n• Сливочное масло — 40 г\n• Соль и сахар — по 1 ч. л.",
                Steps = "1. Замесите мягкое дрожжевое тесто из муки, молока, дрожжей, сахара, соли и одного яйца. Оставьте подняться на 40 минут.\n2. Натрите сыр на терке, добавьте немного воды или молока для сочности.\n3. Сформируйте лодочки с бортиками из теста, наполните сырной начинкой.\n4. Выпекайте при 220°C 15-18 минут.\n5. Перед подачей выпустите желток и кусочек сливочного масла.",
                CuisineId = 2,
                AuthorId = 1,
                ImageUrl = "/uploads/recipes/khachapuri.webp",
                CreatedAt = new DateTime(2026, 10, 22, 14, 30, 0, DateTimeKind.Utc)
            },
            new Recipe
            {
                Id = 3,
                Title = "Традиционный узбекский плов",
                CookingTimeMinutes = 120,
                Servings = 6,
                Ingredients = "• Мякоть говядины или баранины — 1 кг\n• Рис девзира — 1 кг\n• Морковь — 1 кг\n• Лук репчатый — 3 шт.\n• Растительное масло — 250 мл\n• Чеснок — 2 головки\n• Зира — 1.5 ст. л.\n• Барбарис — 1 ст. л.\n• Соль — 2 ст. л.",
                Steps = "1. В раскалённом казане обжарьте лук до золотистого цвета.\n2. Добавьте мясо кусками и обжарьте до румяной корочки.\n3. Добавьте брусочки моркови, зиру и томите 15 минут.\n4. Залейте кипятком, добавьте головки чеснока, барбарис и тушите зирвак 40 минут.\n5. Выложите промытый рис ровным слоем, выпарите воду и доведите под крышкой на углях 25 минут.",
                CuisineId = 3,
                AuthorId = 1,
                ImageUrl = "/uploads/recipes/plov.webp",
                CreatedAt = new DateTime(2026, 10, 25, 16, 0, 0, DateTimeKind.Utc)
            },
            new Recipe
            {
                Id = 4,
                Title = "Французский луковый суп",
                CookingTimeMinutes = 60,
                Servings = 4,
                Ingredients = "• Репчатый лук — 1 кг\n• Сливочное масло — 50 г\n• Оливковое масло — 2 ст. л.\n• Говяжий бульон — 1.2 л\n• Сухое белое вино — 100 мл\n• Французский багет — 8 ломтиков\n• Сыр Грюйер — 150 г\n• Тимьян свежий — 3 веточки",
                Steps = "1. Лук нарежьте тонкими полукольцами.\n2. Карамелизуйте лук на медленном огне 35 минут до глубокого янтарного цвета.\n3. Влейте вино, деглазируйте сковороду, затем влейте горячий говяжий бульон с тимьяном.\n4. Варите суп ещё 20 минут.\n5. Разлейте в керамические горшочки, положите сверху подсушенный багет, засыпьте сыром и запекайте под грилем.",
                CuisineId = 4,
                AuthorId = 2,
                ImageUrl = "/uploads/recipes/onion-soup.webp",
                CreatedAt = new DateTime(2026, 10, 28, 11, 15, 0, DateTimeKind.Utc)
            },
            new Recipe
            {
                Id = 5,
                Title = "Том Ям с тигровыми креветками",
                CookingTimeMinutes = 35,
                Servings = 3,
                Ingredients = "• Бульон — 800 мл\n• Тигровые креветки — 300 г\n• Грибы шампиньоны — 150 г\n• Паста Том Ям — 2 ст. л.\n• Кокосовое молоко — 150 мл\n• Лемонграсс и листья лайма — по вкусу\n• Корень галангала — 20 г\n• Рыбный соус и сок лайма — по 2 ст. л.",
                Steps = "1. В кипящий бульон положите лемонграсс, листья лайма и галангал, варите 5 минут.\n2. Добавьте пасту том ям и тщательно размешайте.\n3. Добавьте нарезанные грибы и помидоры, варите 3 минуты.\n4. Влейте кокосовое молоко, рыбный соус и сок лайма.\n5. Добавьте креветки, варите ровно 2 минуты, снимите с огня и посыпьте свежей кинзой.",
                CuisineId = 5,
                AuthorId = 2,
                ImageUrl = "/uploads/recipes/tom-yam.webp",
                CreatedAt = new DateTime(2026, 10, 30, 18, 0, 0, DateTimeKind.Utc)
            },
            new Recipe
            {
                Id = 6,
                Title = "Тако с маринованной говядиной и сальсой",
                CookingTimeMinutes = 30,
                Servings = 4,
                Ingredients = "• Тортильи кукурузные — 8 шт.\n• Говяжий стейк — 400 г\n• Помидоры спелые — 2 шт.\n• Красный лук — 1 шт.\n• Халапеньо — 1 шт.\n• Лайм — 2 шт.\n• Кинза — 1 пучок\n• Зира и паприка — по 1 ч. л.",
                Steps = "1. Замаринуйте мясо со специями, соком лайма и солью.\n2. Приготовьте свежую сальсу пико-де-гайо из томатов, лука, халапеньо и кинзы.\n3. Обжарьте стейк на гриле до сочности, дайте отдохнуть 5 минут и нарежьте полосками.\n4. Подогрейте лепёшки на сухой сковороде.\n5. Соберите тако: мясо, сальса, сок свежего лайма.",
                CuisineId = 7,
                AuthorId = 1,
                ImageUrl = "/uploads/recipes/tacos.webp",
                CreatedAt = new DateTime(2026, 11, 01, 13, 0, 0, DateTimeKind.Utc)
            },
            // Особое бизнес-правило: Время > 240 минут, шаги > 200 символов:
            new Recipe
            {
                Id = 7,
                Title = "Медленно томлёная говядина Беф Бургиньон по-бургундски",
                CookingTimeMinutes = 270,
                Servings = 6,
                Ingredients = "• Говяжья лопатка — 1.5 кг\n• Бекон копчёный — 150 г\n• Бургундское красное вино — 750 мл\n• Говяжий бульон — 400 мл\n• Морковь — 2 шт.\n• Лук-шалот — 200 г\n• Шампиньоны — 300 г\n• Чеснок — 4 зубчика\n• Томатная паста — 2 ст. л.\n• Мука пшеничная — 2 ст. л.\n• Тимьян и лавровый лист\n• Сливочное масло — 40 г",
                Steps = "1. Вытопите бекон в жаровне, обжарьте кусочки говядины до темно-золотистой корочки.\n2. Добавьте морковь, лук, томатную пасту и муку, прогрейте 3 минуты.\n3. Влейте целую бутылку сухого вина и горячий бульон, положите чеснок и травы.\n4. Закройте плотной крышкой и томите в духовке при 140°C ровно 3.5 часа без открывания.\n5. Отдельно карамелизуйте шалот с грибами на сливочном масле и добавьте к мясу за полчаса до завершения.\n6. Мясо станет нежнейшим, распадаясь от нажатия вилки, а соус станет зеркальным.",
                CuisineId = 4,
                AuthorId = 1,
                ImageUrl = "/uploads/recipes/boeuf-bourguignon.webp",
                CreatedAt = new DateTime(2026, 11, 03, 10, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
