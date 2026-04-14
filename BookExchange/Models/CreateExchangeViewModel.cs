using System.Collections.Generic;

namespace BookExchange.ViewModels
{
    public class CreateExchangeViewModel
    {
        public BookCardViewModel TargetBook { get; set; } = new();
        public List<BookCardViewModel> MyAvailableBooks { get; set; } = new();
    }
}