namespace DealDynamo.Models.HomeViewModels
{
    public class DashBoardViewModel
    {
        public int OrdersCount { get; set; }
        public int UsersCount { get; set; }
        public int SellersCount { get; set; }
        public int ProductsCount { get; set; }
        public int PaymentsCount { get; set; }
        public decimal TotalSales { get; set; }
        public List<MonthlySalesData> MonthlySales { get; set; }
        public List<TopOrderedProductViewModel> TopOrderedProducts { get; set; }
    }
}
