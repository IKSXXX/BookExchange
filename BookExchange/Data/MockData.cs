using System;
using System.Collections.Generic;
using BookExchange.ViewModels;

namespace BookExchange.Data
{
    public static class MockData
    {
        public static List<BookCardViewModel> GetMockBooks()
        {
            return new List<BookCardViewModel>
            {
                new() {
                    Id = "1", Title = "Преступление и наказание", Author = "Ф. Достоевский",
                    CoverUrl = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=400&h=600&fit=crop",
                    Genre = "Классика", Condition = "хорошее", IsAvailable = true,
                    Owner = new OwnerViewModel { Id = "u1", Name = "Анна", Avatar = "https://randomuser.me/api/portraits/women/1.jpg", BooksCount = 12, ExchangesCount = 8 },
                    Description = "Великий роман о преступлении и наказании...", Year = "1866", Language = "Русский"
                },
                new() {
                    Id = "2", Title = "1984", Author = "Дж. Оруэлл",
                    CoverUrl = "https://images.unsplash.com/photo-1543002588-bfa74002ed7e?w=400&h=600&fit=crop",
                    Genre = "Антиутопия", Condition = "отличное", IsAvailable = true,
                    Owner = new OwnerViewModel { Id = "u2", Name = "Игорь", Avatar = "https://randomuser.me/api/portraits/men/2.jpg", BooksCount = 5, ExchangesCount = 3 },
                    Description = "Классическая антиутопия о тоталитаризме...", Year = "1949", Language = "Русский"
                },
                new() {
                    Id = "3", Title = "Мастер и Маргарита", Author = "М. Булгаков",
                    CoverUrl = "https://images.unsplash.com/photo-1544716278-ea5e3a4c8c8c?w=400&h=600&fit=crop",
                    Genre = "Классика", Condition = "удовлетворительное", IsAvailable = false,
                    Owner = new OwnerViewModel { Id = "u3", Name = "Елена", Avatar = "https://randomuser.me/api/portraits/women/3.jpg", BooksCount = 20, ExchangesCount = 15 },
                    Description = "Мистический роман...", Year = "1967", Language = "Русский"
                },
                new() {
                    Id = "4", Title = "Война и мир", Author = "Л. Толстой",
                    CoverUrl = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=400&h=600&fit=crop",
                    Genre = "Классика", Condition = "новая", IsAvailable = true,
                    Owner = new OwnerViewModel { Id = "u1", Name = "Анна", Avatar = "https://randomuser.me/api/portraits/women/1.jpg", BooksCount = 12, ExchangesCount = 8 },
                    Description = "Эпопея о судьбах людей...", Year = "1869", Language = "Русский"
                },
                new() {
                    Id = "5", Title = "Гарри Поттер и философский камень", Author = "Дж. Роулинг",
                    CoverUrl = "https://images.unsplash.com/photo-1543002588-bfa74002ed7e?w=400&h=600&fit=crop",
                    Genre = "Фэнтези", Condition = "хорошее", IsAvailable = true,
                    Owner = new OwnerViewModel { Id = "u4", Name = "Дмитрий", Avatar = "https://randomuser.me/api/portraits/men/4.jpg", BooksCount = 8, ExchangesCount = 4 },
                    Description = "Первая книга о мальчике-волшебнике...", Year = "1997", Language = "Русский"
                },
                new() {
                    Id = "6", Title = "Сто лет одиночества", Author = "Г. Маркес",
                    CoverUrl = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=400&h=600&fit=crop",
                    Genre = "Магический реализм", Condition = "отличное", IsAvailable = true,
                    Owner = new OwnerViewModel { Id = "u5", Name = "Ольга", Avatar = "https://randomuser.me/api/portraits/women/5.jpg", BooksCount = 15, ExchangesCount = 10 },
                    Description = "История семьи Буэндиа...", Year = "1967", Language = "Русский"
                }
            };
        }

        public static List<BookCardViewModel> GetMyBooks()
        {
            // Книги текущего пользователя (для профиля)
            return new List<BookCardViewModel>
            {
                GetMockBooks()[0],
                GetMockBooks()[3],
                GetMockBooks()[4]
            };
        }

        public static OwnerViewModel GetCurrentUser()
        {
            return new OwnerViewModel
            {
                Id = "current",
                Name = "Анна Петрова",
                Avatar = "https://randomuser.me/api/portraits/women/1.jpg",
                BooksCount = 12,
                ExchangesCount = 8,
                Rating = 4.9
            };
        }
    }
}