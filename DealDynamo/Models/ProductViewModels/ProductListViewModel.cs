using DealDynamo.Areas.Identity.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DealDynamo.Models.ProductViewModels
{
    public class ProductListViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }

        public Dictionary<string, string>? Sellers { get; set; }
    }
}
