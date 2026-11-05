using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RecipesApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAuthAndSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AuthorId",
                table: "Recipes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Cuisines",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Итальянская" },
                    { 2, "Грузинская" },
                    { 3, "Узбекская" },
                    { 4, "Французская" },
                    { 5, "Азиатская" },
                    { 6, "Русская" },
                    { 7, "Мексиканская" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "PasswordHash", "Username" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Q2hlZlNhbHRDb29raW5nMTI=.V19rLQs0Bg0wO6HlMEeTjQRwfCBpNrUK1/JuJpGyNMw=", "chef" },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "VXNlclNhbHRDb29raW5nMTI=.qMbnqpURVnyFcequoTbVVtpUFz1z9t7bmuWyfVZ4ZpM=", "testuser" }
                });

            migrationBuilder.InsertData(
                table: "Recipes",
                columns: new[] { "Id", "AuthorId", "CookingTimeMinutes", "CreatedAt", "CuisineId", "ImageUrl", "Ingredients", "Servings", "Steps", "Title" },
                values: new object[,]
                {
                    { 1, 1, 25, new DateTime(2026, 10, 20, 12, 0, 0, 0, DateTimeKind.Utc), 1, "/uploads/recipes/carbonara.webp", "• Спагетти — 200 г\n• Гуанчале или панчетта — 100 г\n• Яичные желтки — 3 шт.\n• Сыр Пармезан или Пекорино — 50 г\n• Свежемолотый чёрный перец — по вкусу\n• Морская соль — для варки", 2, "1. Поставьте воду для пасты закипать, посолите.\n2. Нарежьте гуанчале полосками и обжарьте на сковороде на среднем огне до хрустящей корочки.\n3. В отдельной миске взбейте желтки с натертым сыром и перцем.\n4. Отварите спагетти до состояния al dente, переложите в сковороду с мясом, снимите с огня.\n5. Быстро влейте яично-сырную смесь и немного воды из-под пасты, энергично перемешивая до шелковистого соуса.", "Классическая паста Карбонара" },
                    { 2, 1, 45, new DateTime(2026, 10, 22, 14, 30, 0, 0, DateTimeKind.Utc), 2, "/uploads/recipes/khachapuri.webp", "• Мука пшеничная — 350 г\n• Дрожжи сухие — 5 г\n• Молоко тёплое — 200 мл\n• Сыр сулугуни — 300 г\n• Сыр имеретинский — 150 г\n• Яйца куриные — 3 шт.\n• Сливочное масло — 40 г\n• Соль и сахар — по 1 ч. л.", 2, "1. Замесите мягкое дрожжевое тесто из муки, молока, дрожжей, сахара, соли и одного яйца. Оставьте подняться на 40 минут.\n2. Натрите сыр на терке, добавьте немного воды или молока для сочности.\n3. Сформируйте лодочки с бортиками из теста, наполните сырной начинкой.\n4. Выпекайте при 220°C 15-18 минут.\n5. Перед подачей выпустите желток и кусочек сливочного масла.", "Хачапури по-аджарски" },
                    { 3, 1, 120, new DateTime(2026, 10, 25, 16, 0, 0, 0, DateTimeKind.Utc), 3, "/uploads/recipes/plov.webp", "• Мякоть говядины или баранины — 1 кг\n• Рис девзира — 1 кг\n• Морковь — 1 кг\n• Лук репчатый — 3 шт.\n• Растительное масло — 250 мл\n• Чеснок — 2 головки\n• Зира — 1.5 ст. л.\n• Барбарис — 1 ст. л.\n• Соль — 2 ст. л.", 6, "1. В раскалённом казане обжарьте лук до золотистого цвета.\n2. Добавьте мясо кусками и обжарьте до румяной корочки.\n3. Добавьте брусочки моркови, зиру и томите 15 минут.\n4. Залейте кипятком, добавьте головки чеснока, барбарис и тушите зирвак 40 минут.\n5. Выложите промытый рис ровным слоем, выпарите воду и доведите под крышкой на углях 25 минут.", "Традиционный узбекский плов" },
                    { 4, 2, 60, new DateTime(2026, 10, 28, 11, 15, 0, 0, DateTimeKind.Utc), 4, "/uploads/recipes/onion-soup.webp", "• Репчатый лук — 1 кг\n• Сливочное масло — 50 г\n• Оливковое масло — 2 ст. л.\n• Говяжий бульон — 1.2 л\n• Сухое белое вино — 100 мл\n• Французский багет — 8 ломтиков\n• Сыр Грюйер — 150 г\n• Тимьян свежий — 3 веточки", 4, "1. Лук нарежьте тонкими полукольцами.\n2. Карамелизуйте лук на медленном огне 35 минут до глубокого янтарного цвета.\n3. Влейте вино, деглазируйте сковороду, затем влейте горячий говяжий бульон с тимьяном.\n4. Варите суп ещё 20 минут.\n5. Разлейте в керамические горшочки, положите сверху подсушенный багет, засыпьте сыром и запекайте под грилем.", "Французский луковый суп" },
                    { 5, 2, 35, new DateTime(2026, 10, 30, 18, 0, 0, 0, DateTimeKind.Utc), 5, "/uploads/recipes/tom-yam.webp", "• Бульон — 800 мл\n• Тигровые креветки — 300 г\n• Грибы шампиньоны — 150 г\n• Паста Том Ям — 2 ст. л.\n• Кокосовое молоко — 150 мл\n• Лемонграсс и листья лайма — по вкусу\n• Корень галангала — 20 г\n• Рыбный соус и сок лайма — по 2 ст. л.", 3, "1. В кипящий бульон положите лемонграсс, листья лайма и галангал, варите 5 минут.\n2. Добавьте пасту том ям и тщательно размешайте.\n3. Добавьте нарезанные грибы и помидоры, варите 3 минуты.\n4. Влейте кокосовое молоко, рыбный соус и сок лайма.\n5. Добавьте креветки, варите ровно 2 минуты, снимите с огня и посыпьте свежей кинзой.", "Том Ям с тигровыми креветками" },
                    { 6, 1, 30, new DateTime(2026, 11, 1, 13, 0, 0, 0, DateTimeKind.Utc), 7, "/uploads/recipes/tacos.webp", "• Тортильи кукурузные — 8 шт.\n• Говяжий стейк — 400 г\n• Помидоры спелые — 2 шт.\n• Красный лук — 1 шт.\n• Халапеньо — 1 шт.\n• Лайм — 2 шт.\n• Кинза — 1 пучок\n• Зира и паприка — по 1 ч. л.", 4, "1. Замаринуйте мясо со специями, соком лайма и солью.\n2. Приготовьте свежую сальсу пико-де-гайо из томатов, лука, халапеньо и кинзы.\n3. Обжарьте стейк на гриле до сочности, дайте отдохнуть 5 минут и нарежьте полосками.\n4. Подогрейте лепёшки на сухой сковороде.\n5. Соберите тако: мясо, сальса, сок свежего лайма.", "Тако с маринованной говядиной и сальсой" },
                    { 7, 1, 270, new DateTime(2026, 11, 3, 10, 0, 0, 0, DateTimeKind.Utc), 4, "/uploads/recipes/boeuf-bourguignon.webp", "• Говяжья лопатка — 1.5 кг\n• Бекон копчёный — 150 г\n• Бургундское красное вино — 750 мл\n• Говяжий бульон — 400 мл\n• Морковь — 2 шт.\n• Лук-шалот — 200 г\n• Шампиньоны — 300 г\n• Чеснок — 4 зубчика\n• Томатная паста — 2 ст. л.\n• Мука пшеничная — 2 ст. л.\n• Тимьян и лавровый лист\n• Сливочное масло — 40 г", 6, "1. Вытопите бекон в жаровне, обжарьте кусочки говядины до темно-золотистой корочки.\n2. Добавьте морковь, лук, томатную пасту и муку, прогрейте 3 минуты.\n3. Влейте целую бутылку сухого вина и горячий бульон, положите чеснок и травы.\n4. Закройте плотной крышкой и томите в духовке при 140°C ровно 3.5 часа без открывания.\n5. Отдельно карамелизуйте шалот с грибами на сливочном масле и добавьте к мясу за полчаса до завершения.\n6. Мясо станет нежнейшим, распадаясь от нажатия вилки, а соус станет зеркальным.", "Медленно томлёная говядина Беф Бургиньон по-бургундски" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_AuthorId",
                table: "Recipes",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_Users_AuthorId",
                table: "Recipes",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_Users_AuthorId",
                table: "Recipes");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_AuthorId",
                table: "Recipes");

            migrationBuilder.DeleteData(
                table: "Cuisines",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Cuisines",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Cuisines",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Cuisines",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Cuisines",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Cuisines",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Cuisines",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "Recipes");
        }
    }
}
