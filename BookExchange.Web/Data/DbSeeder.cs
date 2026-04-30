using BookExchange.Db.Data;
using BookExchange.Db.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookExchange.Web.Data;

public static class DbSeeder
{
    public const string AdminRole = "Admin";
    public const string UserRole = "User";
    public const string AdminEmail = "admin@bookswap.com";
    public const string AdminPassword = "Admin123!";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<BookExchangeDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        await ctx.Database.MigrateAsync();

        foreach (var role in new[] { AdminRole, UserRole })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var admin = await userManager.FindByEmailAsync(AdminEmail);
        if (admin == null)
        {
            admin = new User
            {
                UserName = "admin",
                Email = AdminEmail,
                EmailConfirmed = true,
                Location = "Система",
                AvatarPath = "https://randomuser.me/api/portraits/men/0.jpg"
            };
            await userManager.CreateAsync(admin, AdminPassword);
        }
        if (!await userManager.IsInRoleAsync(admin, AdminRole))
            await userManager.AddToRoleAsync(admin, AdminRole);
        if (!await userManager.IsInRoleAsync(admin, UserRole))
            await userManager.AddToRoleAsync(admin, UserRole);

        var demoUsers = new (string userName, string email, string location, string avatar)[]
        {
            ("anna",    "anna@bookswap.com",    "Москва",           "https://randomuser.me/api/portraits/women/1.jpg"),
            ("igor",    "igor@bookswap.com",    "Санкт-Петербург",  "https://randomuser.me/api/portraits/men/2.jpg"),
            ("elena",   "elena@bookswap.com",   "Казань",           "https://randomuser.me/api/portraits/women/3.jpg"),
            ("dmitry",  "dmitry@bookswap.com",  "Новосибирск",      "https://randomuser.me/api/portraits/men/4.jpg"),
            ("olga",    "olga@bookswap.com",    "Екатеринбург",     "https://randomuser.me/api/portraits/women/5.jpg"),
            ("pavel",   "pavel@bookswap.com",   "Нижний Новгород",  "https://randomuser.me/api/portraits/men/6.jpg"),
            ("maria",   "maria@bookswap.com",   "Самара",           "https://randomuser.me/api/portraits/women/7.jpg"),
            ("sergey",  "sergey@bookswap.com",  "Ростов-на-Дону",   "https://randomuser.me/api/portraits/men/8.jpg"),
        };
        var userMap = new Dictionary<string, User>();
        foreach (var (name, email, location, avatar) in demoUsers)
        {
            var u = await userManager.FindByEmailAsync(email);
            if (u == null)
            {
                u = new User
                {
                    UserName = name,
                    Email = email,
                    EmailConfirmed = true,
                    Location = location,
                    AvatarPath = avatar,
                    RegistrationDate = DateTime.UtcNow.AddDays(-Random.Shared.Next(10, 200))
                };
                await userManager.CreateAsync(u, "Pass123!");
                await userManager.AddToRoleAsync(u, UserRole);
            }
            userMap[name] = u;
        }

        if (!await ctx.Books.AnyAsync())
        {
            var books = new List<Book>
            {
                new() { Title = "Преступление и наказание", Author = "Ф. Достоевский", Genre = "Классика", Condition = BookCondition.Good, Year = 1866, Description = "Великий роман о преступлении и наказании, о раскаянии и вере.", CoverImagePath = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=400&h=600&fit=crop", OwnerId = userMap["anna"].Id, IsAvailable = true },
                new() { Title = "1984", Author = "Дж. Оруэлл", Genre = "Антиутопия", Condition = BookCondition.Excellent, Year = 1949, Description = "Классическая антиутопия о тоталитарном обществе.", CoverImagePath = "https://images.unsplash.com/photo-1543002588-bfa74002ed7e?w=400&h=600&fit=crop", OwnerId = userMap["igor"].Id, IsAvailable = true },
                new() { Title = "Мастер и Маргарита", Author = "М. Булгаков", Genre = "Классика", Condition = BookCondition.Acceptable, Year = 1967, Description = "Мистический роман о любви, дьяволе и Москве 30-х.", CoverImagePath = "https://images.unsplash.com/photo-1519682337058-a94d519337bc?w=400&h=600&fit=crop", OwnerId = userMap["elena"].Id, IsAvailable = false },
                new() { Title = "Война и мир", Author = "Л. Толстой", Genre = "Классика", Condition = BookCondition.Excellent, Year = 1869, Description = "Эпопея о судьбах людей на фоне наполеоновских войн.", CoverImagePath = "https://images.unsplash.com/photo-1495446815901-a7297e633e8d?w=400&h=600&fit=crop", OwnerId = userMap["anna"].Id, IsAvailable = true },
                new() { Title = "Гарри Поттер и философский камень", Author = "Дж. Роулинг", Genre = "Фэнтези", Condition = BookCondition.Good, Year = 1997, Description = "Первая книга о мальчике-волшебнике, открывшем мир магии.", CoverImagePath = "https://images.unsplash.com/photo-1609866138210-84bb689f3c5c?w=400&h=600&fit=crop", OwnerId = userMap["dmitry"].Id, IsAvailable = true },
                new() { Title = "Сто лет одиночества", Author = "Г. Маркес", Genre = "Магический реализм", Condition = BookCondition.Excellent, Year = 1967, Description = "История семьи Буэндиа в вымышленном городе Макондо.", CoverImagePath = "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=400&h=600&fit=crop", OwnerId = userMap["olga"].Id, IsAvailable = true },
                new() { Title = "Игра престолов", Author = "Дж. Мартин", Genre = "Фэнтези", Condition = BookCondition.Good, Year = 1996, Description = "Эпическая сага о борьбе за Железный трон Семи Королевств.", CoverImagePath = "https://images.unsplash.com/photo-1532012197267-da84d127e765?w=400&h=600&fit=crop", OwnerId = userMap["pavel"].Id, IsAvailable = true },
                new() { Title = "Евгений Онегин", Author = "А. Пушкин", Genre = "Классика", Condition = BookCondition.Good, Year = 1833, Description = "Роман в стихах, энциклопедия русской жизни.", CoverImagePath = "https://images.unsplash.com/photo-1528459801416-a9e53bbf4e17?w=400&h=600&fit=crop", OwnerId = userMap["maria"].Id, IsAvailable = true },
                new() { Title = "Три товарища", Author = "Э.М. Ремарк", Genre = "Классика", Condition = BookCondition.Acceptable, Year = 1936, Description = "История дружбы и любви в послевоенной Германии.", CoverImagePath = "https://images.unsplash.com/photo-1457369804613-52c61a468e7d?w=400&h=600&fit=crop", OwnerId = userMap["sergey"].Id, IsAvailable = true },
                new() { Title = "Преступные намерения", Author = "А. Кристи", Genre = "Детектив", Condition = BookCondition.Good, Year = 1939, Description = "Классический детектив Агаты Кристи.", CoverImagePath = "https://images.unsplash.com/photo-1476275466078-4007374efbbe?w=400&h=600&fit=crop", OwnerId = userMap["igor"].Id, IsAvailable = true },
                new() { Title = "Отцы и дети", Author = "И. Тургенев", Genre = "Классика", Condition = BookCondition.Good, Year = 1862, Description = "Роман о конфликте поколений и идеологий.", CoverImagePath = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=400&h=600&fit=crop", OwnerId = userMap["elena"].Id, IsAvailable = true },
                new() { Title = "Шерлок Холмс", Author = "А.К. Дойл", Genre = "Детектив", Condition = BookCondition.Excellent, Year = 1892, Description = "Сборник рассказов о гениальном сыщике.", CoverImagePath = "https://images.unsplash.com/photo-1621351183012-e2f9972dd9bf?w=400&h=600&fit=crop", OwnerId = userMap["dmitry"].Id, IsAvailable = true },
                new() { Title = "Анна Каренина", Author = "Л. Толстой", Genre = "Классика", Condition = BookCondition.Good, Year = 1878, Description = "Трагическая история любви в высшем свете Российской империи.", CoverImagePath = "https://images.unsplash.com/photo-1589998059171-988d887df646?w=400&h=600&fit=crop", OwnerId = userMap["anna"].Id, IsAvailable = true },
                new() { Title = "Процесс", Author = "Ф. Кафка", Genre = "Классика", Condition = BookCondition.Acceptable, Year = 1925, Description = "Философский роман о безумии бюрократической системы.", CoverImagePath = "https://images.unsplash.com/photo-1524995997946-a1c2e315a42f?w=400&h=600&fit=crop", OwnerId = userMap["olga"].Id, IsAvailable = true },
                new() { Title = "Заводной апельсин", Author = "Э. Бёрджесс", Genre = "Антиутопия", Condition = BookCondition.Good, Year = 1962, Description = "Провокационный роман о подростке-психопате и социальной инженерии.", CoverImagePath = "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=400&h=600&fit=crop", OwnerId = userMap["pavel"].Id, IsAvailable = true },
                new() { Title = "Великий Гэтсби", Author = "Ф.С. Фицджеральд", Genre = "Классика", Condition = BookCondition.Excellent, Year = 1925, Description = "История любви и разочарования в эпоху джаза.", CoverImagePath = "https://images.unsplash.com/photo-1535905557558-afc4877a26fc?w=400&h=600&fit=crop", OwnerId = userMap["maria"].Id, IsAvailable = true },
                new() { Title = "О дивный новый мир", Author = "О. Хаксли", Genre = "Антиутопия", Condition = BookCondition.Good, Year = 1932, Description = "Антиутопия о мире тотального комфорта и потери человечности.", CoverImagePath = "https://images.unsplash.com/photo-1589998059171-988d887df646?w=400&h=600&fit=crop", OwnerId = userMap["sergey"].Id, IsAvailable = true },
                new() { Title = "Убить пересмешника", Author = "Х. Ли", Genre = "Классика", Condition = BookCondition.Good, Year = 1960, Description = "Глубокий роман о расовых предрассудках и справедливости глазами ребёнка.", CoverImagePath = "https://images.unsplash.com/photo-1543002588-bfa74002ed7e?w=400&h=600&fit=crop", OwnerId = userMap["olga"].Id, IsAvailable = true },
                new() { Title = "Над пропастью во ржи", Author = "Дж. Сэлинджер", Genre = "Классика", Condition = BookCondition.Excellent, Year = 1951, Description = "Исповедь подростка о лицемерии взрослого мира.", CoverImagePath = "https://images.unsplash.com/photo-1519681393784-d120267933ba?w=400&h=600&fit=crop", OwnerId = userMap["anna"].Id, IsAvailable = true }
            };
            await ctx.Books.AddRangeAsync(books);
            await ctx.SaveChangesAsync();

            async Task<int> MakeFakeExchange(string senderId, string receiverId, int bookRequestedId)
            {
                var ex = new ExchangeRequest
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    BookRequestedId = bookRequestedId,
                    Status = ExchangeStatus.Completed
                };
                ctx.ExchangeRequests.Add(ex);
                await ctx.SaveChangesAsync();
                return ex.Id;
            }

            var ex1 = await MakeFakeExchange(userMap["anna"].Id, userMap["igor"].Id, books[1].Id);
            var ex2 = await MakeFakeExchange(userMap["igor"].Id, userMap["anna"].Id, books[0].Id);
            var ex3 = await MakeFakeExchange(userMap["elena"].Id, userMap["dmitry"].Id, books[4].Id);

            ctx.Reviews.AddRange(
                new Review { FromUserId = userMap["anna"].Id,   ToUserId = userMap["igor"].Id,   Rating = 5, Comment = "Отличный обмен, книга пришла в срок!", ExchangeRequestId = ex1 },
                new Review { FromUserId = userMap["igor"].Id,   ToUserId = userMap["anna"].Id,   Rating = 4, Comment = "Спасибо! Книга в хорошем состоянии.", ExchangeRequestId = ex2 },
                new Review { FromUserId = userMap["elena"].Id,  ToUserId = userMap["dmitry"].Id, Rating = 5, Comment = "Дмитрий очень отзывчивый, рекомендую.", ExchangeRequestId = ex3 }
            );
            await ctx.SaveChangesAsync();

            foreach (var u in userMap.Values)
            {
                var rs = await ctx.Reviews.Where(r => r.ToUserId == u.Id).Select(r => r.Rating).ToListAsync();
                u.Rating = rs.Count > 0 ? Math.Round(rs.Average(), 2) : 0;
            }
            await ctx.SaveChangesAsync();

            ctx.QuizQuestions.AddRange(
                new QuizQuestion { BookId = books[0].Id, Quote = "Тварь я дрожащая или право имею?", CorrectAnswer = "Преступление и наказание", Option2 = "Война и мир", Option3 = "Мастер и Маргарита", Option4 = "1984" },
                new QuizQuestion { BookId = books[1].Id, Quote = "Большой Брат следит за тобой.", CorrectAnswer = "1984", Option2 = "Заводной апельсин", Option3 = "Процесс", Option4 = "Игра престолов" },
                new QuizQuestion { BookId = books[2].Id, Quote = "Рукописи не горят.", CorrectAnswer = "Мастер и Маргарита", Option2 = "Преступление и наказание", Option3 = "Анна Каренина", Option4 = "Три товарища" },
                new QuizQuestion { BookId = books[4].Id, Quote = "Да, я волшебник.", CorrectAnswer = "Гарри Поттер и философский камень", Option2 = "Игра престолов", Option3 = "1984", Option4 = "Сто лет одиночества" },
                new QuizQuestion { BookId = books[6].Id, Quote = "Когда играешь в игру престолов — побеждаешь или умираешь.", CorrectAnswer = "Игра престолов", Option2 = "Гарри Поттер и философский камень", Option3 = "Заводной апельсин", Option4 = "Процесс" }
            );
            await ctx.SaveChangesAsync();

            ctx.BooksOfTheDay.Add(new BookOfTheDay { BookId = books[5].Id, Date = DateTime.UtcNow.Date });
            await ctx.SaveChangesAsync();

            var disc = new Discussion { BookId = books[1].Id, UserId = userMap["anna"].Id, Title = "Актуальна ли антиутопия сегодня?" };
            ctx.Discussions.Add(disc);
            await ctx.SaveChangesAsync();
            ctx.DiscussionMessages.AddRange(
                new DiscussionMessage { DiscussionId = disc.Id, UserId = userMap["anna"].Id, Text = "По-моему, книга как никогда актуальна. Что думаете?" },
                new DiscussionMessage { DiscussionId = disc.Id, UserId = userMap["igor"].Id, Text = "Согласен! Особенно про новояз и манипуляции языком." }
            );
            await ctx.SaveChangesAsync();
        }

        var extraBooks = new List<Book>
        {
            new() { Title = "Великий Гэтсби",       Author = "Ф.С. Фицджеральд", Genre = "Классика",   Condition = BookCondition.Excellent, Year = 1925, Description = "История любви и разочарования в эпоху джаза.", CoverImagePath = "https://images.unsplash.com/photo-1535905557558-afc4877a26fc?w=400&h=600&fit=crop", OwnerId = userMap["maria"].Id,  IsAvailable = true },
            new() { Title = "О дивный новый мир",   Author = "О. Хаксли",        Genre = "Антиутопия", Condition = BookCondition.Good,      Year = 1932, Description = "Антиутопия о мире тотального комфорта и потери человечности.", CoverImagePath = "https://images.unsplash.com/photo-1589998059171-988d887df646?w=400&h=600&fit=crop", OwnerId = userMap["sergey"].Id, IsAvailable = true },
            new() { Title = "Убить пересмешника",   Author = "Х. Ли",            Genre = "Классика",   Condition = BookCondition.Good,      Year = 1960, Description = "Глубокий роман о расовых предрассудках и справедливости глазами ребёнка.", CoverImagePath = "https://images.unsplash.com/photo-1543002588-bfa74002ed7e?w=400&h=600&fit=crop", OwnerId = userMap["olga"].Id,   IsAvailable = true },
            new() { Title = "Над пропастью во ржи", Author = "Дж. Сэлинджер",    Genre = "Классика",   Condition = BookCondition.Excellent, Year = 1951, Description = "Исповедь подростка о лицемерии взрослого мира.", CoverImagePath = "https://images.unsplash.com/photo-1519681393784-d120267933ba?w=400&h=600&fit=crop", OwnerId = userMap["anna"].Id,   IsAvailable = true }
        };
        foreach (var b in extraBooks)
        {
            if (!await ctx.Books.AnyAsync(x => x.Title == b.Title))
                ctx.Books.Add(b);
        }
        await ctx.SaveChangesAsync();

        var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
        if (!await ctx.BooksOfTheDay.AnyAsync(b => b.Date == today))
        {
            var anyBook = await ctx.Books.Where(b => !b.IsHidden).OrderBy(b => b.Id).FirstOrDefaultAsync();
            if (anyBook != null)
            {
                ctx.BooksOfTheDay.Add(new BookOfTheDay { BookId = anyBook.Id, Date = today });
                await ctx.SaveChangesAsync();
            }
        }
    }
}
