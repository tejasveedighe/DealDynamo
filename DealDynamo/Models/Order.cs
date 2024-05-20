using DealDynamo.Areas.Identity.Data;
using DealDynamo.Models.Enums;

namespace DealDynamo.Models
{
    public class Order
    {
        public int Id { get; set; }
        public Guid BuyerId { get; set; }
        public Guid SellerId { get; set; }
        public decimal TotalPrice { get; set; }
        public int PaymentId { get; set; }
        public OrderStatusEnum OrderStatus { get; set; }
        public string ShippingAddress { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? ShippingDate { get; set; }

        // Navigation properties
        public List<OrderItems> OrderItems { get; set; }
        public Payments Payment { get; set; }
        public ApplicationUser Buyer { get; set; }
        public ApplicationUser Seller { get; set; }
    }
}
