namespace DealDynamo.Models.OrderViewModels
{
    public class OrderListViewModel
    {
        public IEnumerable<Order> Orders { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
    }
}
