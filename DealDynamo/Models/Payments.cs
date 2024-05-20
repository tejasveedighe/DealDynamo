using DealDynamo.Models.Enums;

namespace DealDynamo.Models
{
    public class Payments
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatusEnum Status { get; set; }
        public DateTime? PaymentDate { get; set; }

        // Navigation property
        public Order Order { get; set; }
    }
}
