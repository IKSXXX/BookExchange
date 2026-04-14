using System.Collections.Generic;

namespace BookExchange.ViewModels
{
    public class BookCatalogViewModel
    {
        public List<BookCardViewModel> Books { get; set; } = new();
        public List<string> AllGenres { get; set; } = new();
        public List<string> SelectedGenres { get; set; } = new();
        public List<string> AllConditions { get; set; } = new();
        public List<string> SelectedConditions { get; set; } = new();
        public bool ShowOnlyAvailable { get; set; }
        public string SearchQuery { get; set; } = "";
        public PaginationInfo Pagination { get; set; } = new();

        public bool HasActiveFilters => !string.IsNullOrEmpty(SearchQuery) ||
                                         SelectedGenres.Count > 0 ||
                                         SelectedConditions.Count > 0 ||
                                         ShowOnlyAvailable;
    }
}