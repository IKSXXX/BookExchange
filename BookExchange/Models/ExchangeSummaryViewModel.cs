namespace BookExchange.ViewModels
{
    public class ExchangeSummaryViewModel
    {
        public string Id { get; set; } = "";
        public string Status { get; set; } = "";
        public OwnerViewModel OtherUser { get; set; } = new();
        public string BookTitle { get; set; } = "";
        public string Date { get; set; } = "";
    }
}