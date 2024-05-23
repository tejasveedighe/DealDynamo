namespace DealDynamo.Models.PaymentViewModels
{
    public class PaymentListViewModel
    {
        public IEnumerable<Payments> Payments { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
    }
}
