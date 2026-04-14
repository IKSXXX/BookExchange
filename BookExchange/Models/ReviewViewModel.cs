namespace BookExchange.ViewModels
{
    public class ReviewViewModel
    {
        public string UserName { get; set; } = "";
        public string Avatar { get; set; } = "";
        public int Rating { get; set; }
        public string Comment { get; set; } = "";
        public string Date { get; set; } = "";
    }
}