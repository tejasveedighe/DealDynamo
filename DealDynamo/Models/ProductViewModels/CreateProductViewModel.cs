using Microsoft.AspNetCore.Mvc.Rendering;

namespace DealDynamo.Models.ProductViewModels
{
    public class CreateProductViewModel
    {
        public string Title { get; set; }
        public int Quantity { get; set; }
        public string? Description { get; set; }
        public IFormFile ProductImage { get; set; }
        public SelectList Category { get; set; }
        public int Price { get; set; }

    }
}
