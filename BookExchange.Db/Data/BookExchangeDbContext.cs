using BookExchange.Db.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookExchange.Db.Data;

/// <summary>
/// Контекст базы данных — "мост" между EF Core и СУБД (PostgreSQL).
/// Наследуется от <see cref="IdentityDbContext{TUser}"/>, чтобы получить готовые таблицы
/// AspNetUsers / AspNetRoles / AspNetUserRoles и т.д. (Identity).
/// </summary>
public class BookExchangeDbContext : IdentityDbContext<User>
{
    public BookExchangeDbContext(DbContextOptions<BookExchangeDbContext> options)
        : base(options)
    {
    }

    // DbSet<TEntity> — это "таблица" в коде. EF Core по этим свойствам строит схему.
    public DbSet<Book> Books => Set<Book>();
    public DbSet<ExchangeRequest> ExchangeRequests => Set<ExchangeRequest>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Discussion> Discussions => Set<Discussion>();
    public DbSet<DiscussionMessage> DiscussionMessages => Set<DiscussionMessage>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<BookOfTheDay> BooksOfTheDay => Set<BookOfTheDay>();

    /// <summary>
    /// Fluent API-конфигурация. Тут задаём связи, индексы, уникальные ограничения
    /// там, где атрибутов недостаточно или они неудобны.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder b)
    {
        // ВАЖНО: у IdentityDbContext базовый OnModelCreating настраивает Identity-таблицы.
        // Его обязательно нужно вызвать ПЕРВЫМ, иначе Identity-схема не создастся.
        base.OnModelCreating(b);

        // ---------- Book ----------
        b.Entity<Book>(e =>
        {
            e.HasOne(x => x.Owner)
             .WithMany(u => u.Books)
             .HasForeignKey(x => x.OwnerId)
             .OnDelete(DeleteBehavior.Cascade); // удалили пользователя — удалились его книги
            e.HasIndex(x => x.Genre);
            e.HasIndex(x => x.Title);
        });

        // ---------- ExchangeRequest ----------
        // Два FK от одной таблицы (Sender/Receiver оба User) требуют явного Restrict,
        // иначе PostgreSQL/EF ругается на "multiple cascade paths".
        b.Entity<ExchangeRequest>(e =>
        {
            e.HasOne(x => x.Sender)
             .WithMany(u => u.SentRequests)
             .HasForeignKey(x => x.SenderId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Receiver)
             .WithMany(u => u.ReceivedRequests)
             .HasForeignKey(x => x.ReceiverId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.BookRequested)
             .WithMany()
             .HasForeignKey(x => x.BookRequestedId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.BookOffered)
             .WithMany()
             .HasForeignKey(x => x.BookOfferedId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ---------- Message (чат в обмене) ----------
        b.Entity<Message>(e =>
        {
            e.HasOne(x => x.ExchangeRequest)
             .WithMany(r => r.Messages)
             .HasForeignKey(x => x.ExchangeRequestId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Sender)
             .WithMany()
             .HasForeignKey(x => x.SenderId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Review ----------
        b.Entity<Review>(e =>
        {
            e.HasOne(x => x.FromUser)
             .WithMany(u => u.ReviewsGiven)
             .HasForeignKey(x => x.FromUserId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.ToUser)
             .WithMany(u => u.ReviewsReceived)
             .HasForeignKey(x => x.ToUserId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.ExchangeRequest)
             .WithMany(r => r.Reviews)
             .HasForeignKey(x => x.ExchangeRequestId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- WishlistItem / Favorite ----------
        // Запрещаем дублировать одну и ту же пару (UserId, BookId) — уникальный индекс.
        b.Entity<WishlistItem>(e =>
        {
            e.HasIndex(x => new { x.UserId, x.BookId }).IsUnique();
            e.HasOne(x => x.User).WithMany(u => u.WishlistItems).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Book).WithMany(bk => bk.WishlistedBy).HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Cascade);
        });
        b.Entity<Favorite>(e =>
        {
            e.HasIndex(x => new { x.UserId, x.BookId }).IsUnique();
            e.HasOne(x => x.User).WithMany(u => u.Favorites).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Book).WithMany(bk => bk.FavoritedBy).HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- Discussion / DiscussionMessage ----------
        b.Entity<Discussion>(e =>
        {
            e.HasOne(x => x.Book).WithMany(bk => bk.Discussions).HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User).WithMany(u => u.Discussions).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });
        b.Entity<DiscussionMessage>(e =>
        {
            e.HasOne(x => x.Discussion).WithMany(d => d.Messages).HasForeignKey(x => x.DiscussionId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User).WithMany(u => u.DiscussionMessages).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- QuizQuestion ----------
        b.Entity<QuizQuestion>(e =>
        {
            e.HasOne(x => x.Book).WithMany(bk => bk.QuizQuestions).HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- BookOfTheDay ----------
        b.Entity<BookOfTheDay>(e =>
        {
            e.HasIndex(x => x.Date).IsUnique();
            e.HasOne(x => x.Book).WithMany().HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// Автоматически проставляем <c>UpdatedAt</c> у всех изменённых сущностей при сохранении.
    /// Это экономит код в репозиториях — не нужно везде руками писать entity.UpdatedAt = now.
    /// </summary>
    public override int SaveChanges()
    {
        StampAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void StampAuditFields()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedAt == default) entry.Entity.CreatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}
