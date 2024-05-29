namespace DealDynamo.Models.ProductViewModels
{
    public class ProductCardListViewModel
    {
        public List<Product> Products { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public string SearchText { get; set; }
        public int? CategoryFilter { get; set; }
        public string SortOption { get; set; }
    }

}
