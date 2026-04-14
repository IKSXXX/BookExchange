using BookExchange.ViewModels;

namespace BookExchange.ViewModels
{
    public class BookCardViewModel
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string CoverUrl { get; set; } = "";
        public string Genre { get; set; } = "";
        public string Condition { get; set; } = "";
        public bool IsAvailable { get; set; } = true;
        public OwnerViewModel Owner { get; set; } = new();
        public string Description { get; set; } = "";
        public string Year { get; set; } = "";
        public string Language { get; set; } = "Русский";
    }
}