using Microsoft.AspNetCore.Mvc;
using BookExchange.ViewModels;
using BookExchange.Data;

namespace BookExchange.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var model = new HomeViewModel
            {
                RecentBooks = MockData.GetMockBooks().Take(6).ToList()
            };
            return View(model);
        }
    }
}