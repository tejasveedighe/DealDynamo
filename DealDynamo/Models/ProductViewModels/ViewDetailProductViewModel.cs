using System.ComponentModel.DataAnnotations;

namespace DealDynamo.Models.ProductViewModels
{
    public class ViewDetailProductViewModel
    {
        [Key]
        public int ProductId { get; set; }
        public string Title { get; set; }
        public int Quantity { get; set; }
        public string? Description { get; set; }
        [Display(Name = "Product Image")]
        public string ProductImage { get; set; }
        public Category Category { get; set; }
        public int Price { get; set; }
    }
}
