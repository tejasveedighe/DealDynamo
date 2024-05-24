using DealDynamo.Areas.Identity.Data;
using DealDynamo.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace DealDynamo.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string BuyerId { get; set; }
        [Display(Name = "Total Price")]
        public decimal TotalPrice { get; set; }
        public int? PaymentId { get; set; }
        [Display(Name = "Order Status")]
        public OrderStatusEnum OrderStatus { get; set; }
        public int AddressId { get; set; }
        [Display(Name = "Order Date")]
        public DateTime? OrderDate { get; set; }
        [Display(Name = "Delivery Date")]
        public DateTime? ShippingDate { get; set; }

        // Navigation properties
        public List<OrderItems> OrderItems { get; set; }
        public Payments Payment { get; set; }
        public ApplicationUser Buyer { get; set; }
        public Address Address { get; set; }
        DealDynamo
    }
}
