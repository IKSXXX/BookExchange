# BookSwap — платформа обмена книгами

Веб-приложение на **ASP.NET Core 8 MVC + PostgreSQL + SignalR**, где пользователи могут выложить свои книги, найти интересующие, предложить обмен и пообщаться в реальном времени.

> Это учебный/демо-проект. Код обильно прокомментирован — он рассчитан на разработчика, только входящего в мир ASP.NET Core.

---

## Оглавление

1. [Функциональность](#функциональность)
2. [Технологии](#технологии)
3. [Установка на Windows](#установка-на-windows)
4. [Первый запуск](#первый-запуск)
5. [Тестовые аккаунты](#тестовые-аккаунты)
6. [Архитектура](#архитектура)
7. [Как всё устроено (по папкам)](#как-всё-устроено-по-папкам)
8. [Как добавить новую фичу](#как-добавить-новую-фичу)
9. [FAQ](#faq)

---

## Функциональность

- Регистрация, вход, выход (**ASP.NET Core Identity**).
- Личный профиль с аватаром, городом, рейтингом.
- Каталог книг с поиском (debounce 500 мс), фильтрами (жанр, состояние, доступность) и пагинацией.
- Карточка книги: описание, владелец, похожие книги, обсуждения (мини-форум в реальном времени).
- Обмен книгами: заявка, статусы (Pending → Accepted/Rejected → Completed/Cancelled), чат участников через SignalR, отзыв после завершения.
- Избранное, вишлист.
- Главная страница: карусель, книга дня, случайная книга, мини-игра «угадай книгу по цитате», AI-виджет «что почитать?» (наивный поиск по ключевым словам — заглушка под будущий OpenAI/Gemini).
- Админ-панель (роль `Admin`): пользователи (блок/разблок, назначение админом), модерация книг, статистика, выбор книги дня, CRUD вопросов для игры.

## Технологии

| Слой | Технология |
|------|-----------|
| Web  | ASP.NET Core 8 MVC (Razor + Tag Helpers) |
| ORM  | Entity Framework Core 8 (Code First) |
| БД   | PostgreSQL 14+ (через `Npgsql.EntityFrameworkCore.PostgreSQL`) |
| Auth | ASP.NET Core Identity (cookie-based) |
| Realtime | SignalR (хабы `ChatHub`, `DiscussionHub`) |
| Mapping | AutoMapper 13 |
| UI | Bootstrap 5 + кастомный CSS (`wwwroot/css/site.css`) |
| JS | Чистый ES6 (без jQuery) |

## Установка на Windows

### 1. Установить .NET 8 SDK

Скачать: <https://dotnet.microsoft.com/download/dotnet/8.0> → «Windows x64 Installer». Проверить:

```powershell
dotnet --version   # должно быть 8.x.x
```

### 2. Установить PostgreSQL

1. Скачать: <https://www.postgresql.org/download/windows/> (EnterpriseDB Installer).
2. При установке задать пароль для пользователя `postgres` — запомни его.
3. Вместе с PostgreSQL установится **pgAdmin 4** — удобный GUI-клиент БД.

### 3. Создать базу данных

Открыть **pgAdmin 4** → Servers → PostgreSQL → правой кнопкой по «Databases» → Create → Database…
Имя БД: `bookswap`. Владельца оставить `postgres`.

Либо в командной строке:

```powershell
& "C:\Program Files\PostgreSQL\16\bin\psql.exe" -U postgres -c "CREATE DATABASE bookswap;"
```

### 4. Клонировать репозиторий

```powershell
git clone https://github.com/IKSXXX/BookExchange.git
cd BookExchange
```

### 5. Настроить строку подключения

Открыть `BookExchange.Web/appsettings.json`. Если пароль `postgres` у тебя другой — поменяй его тут:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=bookswap;Username=postgres;Password=postgres"
}
```

> Хочешь не коммитить пароль? Положи `appsettings.Development.json` рядом (уже в `.gitignore`) с такой же секцией `ConnectionStrings`.

### 6. Установить инструмент EF Core (один раз на машину)

```powershell
dotnet tool install --global dotnet-ef --version 8.0.10
```

## Первый запуск

```powershell
# 1. Применить миграции (создаст все таблицы и Identity-схему)
dotnet ef database update --project BookExchange.Db --startup-project BookExchange.Web

# 2. Запустить сайт
dotnet run --project BookExchange.Web
```

Приложение поднимется на `http://localhost:5000` (или на порту из `launchSettings.json`). При первом запуске сработает **DbSeeder** — создаст роли, админа, 8 тестовых пользователей, 16 книг, отзывы, вопросы игры и обсуждение. Смотри логи в консоли.

### Повторная миграция после изменений модели

```powershell
dotnet ef migrations add ИмяМиграции --project BookExchange.Db --startup-project BookExchange.Web
dotnet ef database update           --project BookExchange.Db --startup-project BookExchange.Web
```

## Тестовые аккаунты

| Роль | Email | Пароль |
|------|-------|--------|
| Admin | `admin@bookswap.com` | `Admin123!` |
| User | `anna@bookswap.com` | `Pass123!` |
| User | `igor@bookswap.com` | `Pass123!` |
| User | `elena@bookswap.com` | `Pass123!` |
| User | `dmitry@bookswap.com` | `Pass123!` |
| User | `olga@bookswap.com` | `Pass123!` |
| User | `pavel@bookswap.com` | `Pass123!` |
| User | `maria@bookswap.com` | `Pass123!` |
| User | `sergey@bookswap.com` | `Pass123!` |

## Архитектура

Два проекта в одном solution (`BookExchange.sln`):

```
BookExchange.sln
├── BookExchange.Db/        ← библиотека: сущности, DbContext, репозитории, миграции
└── BookExchange.Web/       ← веб-приложение: контроллеры, Razor views, SignalR, DI
```

Зачем разделение? Если завтра захочешь добавить фоновый сервис или API на том же коде моделей — просто подключаешь `BookExchange.Db` как `ProjectReference`, и всё. Слой данных не зависит от веб-стека.

### Паттерн Repository + Unit of Work

`IRepository<T>` — универсальный набор CRUD-методов.
`IUnitOfWork` — фасад над всеми репозиториями + `SaveChangesAsync()`.
В контроллерах используется только `IUnitOfWork` (а не DbContext напрямую) — это помогает писать тесты и держать слои разделёнными.

```csharp
// Пример использования в контроллере:
var book = await _uow.Books.GetByIdAsync(id);
_uow.Favorites.Add(new Favorite { UserId = uid, BookId = id });
await _uow.SaveChangesAsync();
```

### SignalR

- **ChatHub** (`/hubs/chat`) — чат участников внутри заявки на обмен.
- **DiscussionHub** (`/hubs/discussion`) — обсуждения книг.

Каждая заявка/тема — отдельная **группа** SignalR (`exchange-{id}` или `discussion-{id}`), в которую попадают только участники. Сообщения сохраняются в БД и одновременно рассылаются всем подключённым клиентам группы.

## Как всё устроено (по папкам)

### `BookExchange.Db`

| Папка | Что там |
|-------|---------|
| `Entities/` | 12 сущностей: `User`, `Book`, `ExchangeRequest`, `Message`, `Review`, `Favorite`, `WishlistItem`, `Discussion`, `DiscussionMessage`, `QuizQuestion`, `BookOfTheDay` + `BaseEntity`. |
| `Data/BookExchangeDbContext.cs` | Наследник `IdentityDbContext<User>`. Fluent API настраивает все связи и каскадные удаления. |
| `Interfaces/` | `IRepository<T>`, `IUnitOfWork`. |
| `Repositories/` | `Repository<T>`, `UnitOfWork` (ленивая инициализация — каждый репозиторий создаётся по первому обращению). |
| `Migrations/` | Автогенерируемые миграции EF Core (не редактируй вручную). |

### `BookExchange.Web`

| Папка | Что там |
|-------|---------|
| `Controllers/` | 9 контроллеров: `HomeController`, `AccountController`, `BookController`, `ExchangeController`, `DiscussionController`, `FavoriteController`, `WishlistController`, `UserController`, `AdminController`. |
| `ViewModels/` | Модели для передачи в Razor (отделены от Entity, чтобы не светить лишние поля клиенту). |
| `Views/` | Razor-шаблоны по папкам контроллеров + `Shared/_Layout`, `Shared/_BookCard`. |
| `Hubs/` | `ChatHub`, `DiscussionHub` — SignalR. |
| `Helpers/` | `MappingProfile` (AutoMapper), `ImageHelper` (загрузка обложек/аватаров), `GoogleBooksHelper` (заглушка), `AIHelper` (заглушка), `EmailSender` (вывод в консоль). |
| `Data/DbSeeder.cs` | Создание ролей, админа, тестовых данных при первом запуске. |
| `wwwroot/` | Статика: `css/site.css`, `js/site.js`, `lib/` (Bootstrap, jquery-validation). |
| `Program.cs` | Единая точка конфигурации: DI, Identity, EF Core, SignalR, middleware, маршруты. |
| `appsettings.json` | Конфиг (строка подключения, уровни логов). |

### Ключевые места в `Program.cs`

```csharp
// БД
builder.Services.AddDbContext<BookExchangeDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity с кастомным User
builder.Services.AddIdentity<User, IdentityRole>(options => { /* правила пароля */ })
    .AddEntityFrameworkStores<BookExchangeDbContext>();

// Репозитории
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

// SignalR
builder.Services.AddSignalR();
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<DiscussionHub>("/hubs/discussion");

// Seed
await DbSeeder.SeedAsync(app.Services);
```

## Как добавить новую фичу

### Пример: добавить поле «Издательство» в книгу

1. `BookExchange.Db/Entities/Book.cs` → добавить `public string? Publisher { get; set; }`.
2. Создать миграцию:
   ```powershell
   dotnet ef migrations add BookPublisher --project BookExchange.Db --startup-project BookExchange.Web
   dotnet ef database update               --project BookExchange.Db --startup-project BookExchange.Web
   ```
3. В `BookExchange.Web/ViewModels/BookViewModels.cs` → добавить поле в `BookFormViewModel` и `BookDetailsViewModel`.
4. В `Helpers/MappingProfile.cs` → AutoMapper подхватит одноимённые поля автоматически.
5. В `Views/Book/Form.cshtml` → добавить `<input asp-for="Publisher" />`.
6. В `Views/Book/Details.cshtml` → отобразить `@Model.Publisher`.

## FAQ

**Вижу ошибку `FATAL: password authentication failed for user "postgres"` при миграциях.**
Пароль в `appsettings.json` не совпадает с паролем твоего пользователя `postgres` в PostgreSQL. Поменяй строку подключения — либо пароль пользователя через `ALTER USER postgres WITH PASSWORD 'postgres';` в `psql`.

**Миграции применились, но при входе ошибка `relation "AspNetUsers" does not exist`.**
Проверь, что `dotnet ef database update` отработал **в ту же БД**, что указана в `appsettings.json` (имя БД в строке подключения vs имя БД, которую ты смотришь в pgAdmin).

**Как удалить всё и начать заново?**
```powershell
dotnet ef database drop -f --project BookExchange.Db --startup-project BookExchange.Web
dotnet ef database update   --project BookExchange.Db --startup-project BookExchange.Web
```

**Не приходит SignalR — сообщения появляются только после перезагрузки страницы.**
Открой DevTools → Network → отфильтруй по `hubs/chat` — должен быть 101 Switching Protocols. Если нет, значит WebSocket заблокирован (редкий случай на Windows — проверь антивирус/firewall) или клиент не попал в группу (смотри вкладку Console).

**Как сменить порт?**
`BookExchange.Web/Properties/launchSettings.json` → `applicationUrl`.

**Куда грузятся загруженные обложки?**
`BookExchange.Web/wwwroot/uploads/...` — статика отдаётся напрямую, URL вида `/uploads/covers/{guid}.jpg` сохраняется в БД в `Book.CoverImagePath`.
