using System.Collections.Generic;

namespace BookExchange.ViewModels
{
    public class HomeViewModel
    {
        public List<BookCardViewModel> RecentBooks { get; set; } = new();
    }
}