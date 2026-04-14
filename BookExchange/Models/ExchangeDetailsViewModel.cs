using System.Collections.Generic;

namespace BookExchange.ViewModels
{
    public class ExchangeDetailsViewModel
    {
        public string Id { get; set; } = "";
        public string Status { get; set; } = "pending"; // pending, accepted, rejected, completed
        public OwnerViewModel OtherUser { get; set; } = new();
        public List<BookCardViewModel> TheirBooks { get; set; } = new();
        public List<BookCardViewModel> MyOfferedBooks { get; set; } = new();
        public List<ChatMessageViewModel> Messages { get; set; } = new();
    }
}