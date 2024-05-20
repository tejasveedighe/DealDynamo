namespace DealDynamo.Models
{
    public class OrderItems
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public decimal PricePerUnit { get; set; }
        public int Quantity { get; set; }

        // Navigation property
        public Order Order { get; set; }
        public Product Product { get; set; }
    }
}
