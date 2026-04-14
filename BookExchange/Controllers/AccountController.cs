using Microsoft.AspNetCore.Mvc;
using BookExchange.ViewModels;
using BookExchange.Data;

namespace BookExchange.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Profile()
        {
            var currentUser = MockData.GetCurrentUser();
            var model = new ProfileViewModel
            {
                CurrentUser = new UserProfileViewModel
                {
                    User = currentUser,
                    Books = MockData.GetMyBooks()
                },
                MyBooks = MockData.GetMyBooks(),
                Exchanges = new System.Collections.Generic.List<ExchangeSummaryViewModel>() // заглушка
            };
            return View(model);
        }

        public IActionResult Login() => Content("Страница входа будет позже");
        public IActionResult Register() => Content("Страница регистрации будет позже");
        public IActionResult Logout() => RedirectToAction("Index", "Home");
    }

    public class UserController : Controller
    {
        public IActionResult Profile(string id)
        {
            var userBooks = MockData.GetMockBooks().Where(b => b.Owner.Id == id).ToList();
            var user = userBooks.FirstOrDefault()?.Owner;
            if (user == null) return NotFound();

            var model = new UserProfileViewModel
            {
                User = user,
                Books = userBooks
            };
            return View(model);
        }
    }
}