using DealDynamo.Areas.Identity.Data;

namespace DealDynamo.Models
{
    public class AppCartItem
    {
        public int Id { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
    }
}
