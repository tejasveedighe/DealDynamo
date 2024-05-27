using DealDynamo.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace DealDynamo.Models
{
    public class Payments
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatusEnum Status { get; set; }
        [Display(Name = "Payment Date")]
        public DateTime? PaymentDate { get; set; }
        public string? StripePaymentId { get; set; }

        // Navigation property
        public Order Order { get; set; }
    }
}
