namespace DealDynamo.Models.CartViewModels
{
    public class CartViewModel
    {
        public List<AppCartItem> CartItems { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
