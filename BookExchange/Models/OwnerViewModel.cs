namespace BookExchange.ViewModels
{
    public class OwnerViewModel
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Avatar { get; set; } = "";
        public int BooksCount { get; set; }
        public int ExchangesCount { get; set; }
        public double Rating { get; set; } = 4.8;
    }
}