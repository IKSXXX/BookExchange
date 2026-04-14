using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using BookExchange.ViewModels;
using BookExchange.Data;

namespace BookExchange.Controllers
{
    public class ExchangeController : Controller
    {
        public IActionResult Create(string bookId)
        {
            var targetBook = MockData.GetMockBooks().FirstOrDefault(b => b.Id == bookId);
            if (targetBook == null) return NotFound();

            var model = new CreateExchangeViewModel
            {
                TargetBook = targetBook,
                MyAvailableBooks = MockData.GetMyBooks().Where(b => b.IsAvailable).ToList()
            };
            return View(model);
        }

        public IActionResult Details(string id)
        {
            // Заглушка
            var model = new ExchangeDetailsViewModel
            {
                Id = id,
                Status = "accepted",
                OtherUser = new OwnerViewModel { Id = "u2", Name = "Игорь Смирнов", Avatar = "https://randomuser.me/api/portraits/men/2.jpg" },
                TheirBooks = MockData.GetMockBooks().Take(1).ToList(),
                MyOfferedBooks = MockData.GetMyBooks().Take(2).ToList(),
                Messages = new System.Collections.Generic.List<ChatMessageViewModel>()
            };
            return View(model);
        }
    }
}