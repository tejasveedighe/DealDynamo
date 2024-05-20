namespace DealDynamo.Models.OrderViewModels
{
    public class OrderCheckoutViewModel
    {
        public List<AppCartItem> CartItems { get; set; }
        public Address Address { get; set; }
    }
}
