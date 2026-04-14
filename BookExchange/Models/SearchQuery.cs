namespace BookExchange.ViewModels
{
    public class SearchQuery
    {
        public string Search { get; set; } = "";
        public string GenreId { get; set; } = "";
        public string Condition { get; set; } = "";
        public string City { get; set; } = "";
        public string SortOrder { get; set; } = "default";
    }
}