using System.Collections.Generic;

namespace BookExchange.ViewModels
{
    public class UserProfileViewModel
    {
        public OwnerViewModel User { get; set; } = new();
        public List<BookCardViewModel> Books { get; set; } = new();
    }
}