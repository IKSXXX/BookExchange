using BookExchange.ViewModels;
using System.Collections.Generic;

namespace BookExchange.ViewModels
{
    public class BookDetailsViewModel
    {
        public BookCardViewModel Book { get; set; } = new();
        public OwnerViewModel Owner { get; set; } = new();
        public bool IsCurrentUserOwner { get; set; }
        public List<BookCardViewModel> SimilarBooks { get; set; } = new();
        public List<ReviewViewModel> Reviews { get; set; } = new();
        public string Id => Book?.Id ?? "";
        public string Title => Book?.Title ?? "";
        public string Author => Book?.Author ?? "";
        public string CoverUrl => Book?.CoverUrl ?? "";
        public string Genre => Book?.Genre ?? "";
        public string Condition => Book?.Condition ?? "";
        public bool IsAvailable => Book?.IsAvailable ?? false;
        public string Description => Book?.Description ?? "";
        public string Year => Book?.Year ?? "";
        public string Language => Book?.Language ?? "";
    }
}