namespace DealDynamo.Models.ReviewViewModels
{
    public class ReviewListViewModel
    {
        public IEnumerable<ProductReview> Reviews { get; set; }

        public string BuyerNameFilter { get; set; }
        public string ProductNameFilter { get; set; }
        public int? RatingFilter { get; set; }
        public DateTime? DateSubmittedFilter { get; set; }

        public string SortBy { get; set; }
        public bool IsSortAscending { get; set; }

        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
