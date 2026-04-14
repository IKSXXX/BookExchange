using System.Collections.Generic;

namespace BookExchange.ViewModels
{
    public class ProfileViewModel
    {
        public UserProfileViewModel CurrentUser { get; set; } = new();
        public List<BookCardViewModel> MyBooks { get; set; } = new();
        public List<ExchangeSummaryViewModel> Exchanges { get; set; } = new();
    }
}