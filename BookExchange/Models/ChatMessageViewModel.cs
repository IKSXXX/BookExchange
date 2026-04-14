using System;

namespace BookExchange.ViewModels
{
    public class ChatMessageViewModel
    {
        public string Id { get; set; } = "";
        public string SenderId { get; set; } = "";
        public string Text { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public bool IsMine => SenderId == "current"; // упрощённо
    }
}