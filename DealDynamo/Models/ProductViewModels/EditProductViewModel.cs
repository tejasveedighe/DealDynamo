using System.ComponentModel.DataAnnotations;

namespace DealDynamo.Models.ProductViewModels
{
    public class EditProductViewModel
    {
        public int ProductId { get; set; }
        public string Title { get; set; }

        [Range(minimum: 1, maximum: 100000)]
        public int Quantity { get; set; }
        public string? Description { get; set; }

        [Display(Name = "Product Image")]
        public string ProductImage { get; set; }

        [DataType(DataType.Upload)]
        [Display(Name = "Update Product Image")]
        public IFormFile NewProductImage { get; set; }
        public IEnumerable<Category> Categories { get; set; }

        [Required(ErrorMessage = "Category Must be Selected")]
        public int? CategoryId { get; set; }

        [DataType(DataType.Currency)]
        public int Price { get; set; }

    }
}
