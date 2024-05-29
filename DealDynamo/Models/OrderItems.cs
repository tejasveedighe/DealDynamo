using DealDynamo.Areas.Identity.Data;
using DealDynamo.Models.Enums;

namespace DealDynamo.Models
{
    public class OrderItems
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public decimal PricePerUnit { get; set; }
        public int Quantity { get; set; }
        public OrderItemStatus Status { get; set; }

        // Navigation property
        public Order Order { get; set; }
        public Product Product { get; set; }
        public ApplicationUser Seller { get; set; }
        public string SellerId { get; set; }
    }
}
