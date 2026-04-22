// =====================================================================================
// Program.cs — точка входа ASP.NET Core 8.
// Здесь мы:
//   1) настраиваем сервисы (Dependency Injection): DbContext, Identity, AutoMapper, SignalR, MVC.
//   2) собираем приложение (app) и настраиваем middleware-пайплайн.
//   3) сидируем базу (роли, админ, тестовые данные).
// =====================================================================================

using BookExchange.Db.Data;
using BookExchange.Db.Entities;
using BookExchange.Db.Interfaces;
using BookExchange.Db.Repositories;
using BookExchange.Web.Data;
using BookExchange.Web.Helpers;
using BookExchange.Web.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// -------------------- 1. DbContext (PostgreSQL через Npgsql) --------------------
// Строка подключения берётся из appsettings.json (ConnectionStrings:DefaultConnection).
// Пример для Windows: "Host=localhost;Port=5432;Database=bookswap;Username=postgres;Password=ВАШ_ПАРОЛЬ"
var connString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Строка подключения 'DefaultConnection' не задана в appsettings.json");
builder.Services.AddDbContext<BookExchangeDbContext>(options => options.UseNpgsql(connString));

// -------------------- 2. Identity (регистрация/вход/роли) --------------------
builder.Services
    .AddIdentity<User, IdentityRole>(options =>
    {
        // Упростили политику пароля для демо. В продакшене требуйте большей длины/символов!
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireUppercase = true;
        options.Password.RequireDigit = true;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false; // подтверждение email не требуем
    })
    .AddEntityFrameworkStores<BookExchangeDbContext>()
    .AddDefaultTokenProviders();

// Перенаправление на наш собственный AccountController вместо scaffold-страниц Identity.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
});

// -------------------- 3. Репозитории / UoW / Helpers --------------------
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
builder.Services.AddTransient<IEmailSender, ConsoleEmailSender>();

// -------------------- 4. MVC + SignalR --------------------
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

var app = builder.Build();

// -------------------- 5. HTTP pipeline --------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// На dev-машине HTTPS-редирект обычно мешает, включим только в продакшне.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

// Порядок важен: сперва Authentication, потом Authorization.
app.UseAuthentication();
app.UseAuthorization();

// Маршруты:
//  - /Admin/... через "area" Admin (если нужно, но мы делаем обычный AdminController в корне)
//  - default: {controller=Home}/{action=Index}/{id?}
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// SignalR-хабы
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<DiscussionHub>("/hubs/discussion");

// -------------------- 6. Сидирование БД --------------------
// Делается ДО app.Run(). Если миграции ещё не применялись — применятся автоматически.
// Для продакшена обычно это делают отдельной утилитой, но для учебного проекта — нормально.
await DbSeeder.SeedAsync(app.Services);

app.Run();
