using DealDynamo.Areas.Identity.Data;
using DealDynamo.Data;
using DealDynamo.Models;
using DealDynamo.Models.HomeViewModels;
using DealDynamo.Models.ProductViewModels;
using DealDynamo.Services;
using MailKit.Search;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Globalization;

namespace DealDynamo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> UserManager;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentsRepository _paymentsRepository;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, IProductRepository productRepository, ICategoryRepository categoryRepository, IOrderRepository orderRepository, IPaymentsRepository paymentsRepository)
        {
            _logger = logger;
            UserManager = userManager;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _orderRepository = orderRepository;
            _paymentsRepository = paymentsRepository;
        }

        public async Task<IActionResult> Index(string searchText, int? categoryFilter, string sortOption, int currentPage = 1, int pageSize = 10)
        {
            var user = await UserManager.GetUserAsync(User);

            if (!User.Identity.IsAuthenticated || (user != null && user.IsBuyer))
            {
                ViewBag.Categories = _categoryRepository.GetAllCategories().ToList();

                var products = _productRepository.GetAllProducts();

                if (!string.IsNullOrEmpty(searchText))
                {
                    products = products.Where(p => p.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (categoryFilter.HasValue)
                {
                    products = products.Where(p => p.CategoryID == categoryFilter.Value).ToList();
                }

                switch (sortOption)
                {
                    case "name":
                        products = products.OrderBy(p => p.Title).ToList();
                        break;
                    case "price":
                        products = products.OrderBy(p => p.Price).ToList();
                        break;
                }

                var totalProducts = products.Count();
                var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

                var paginatedProducts = products.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

                var vm = new ProductCardListViewModel()
                {
                    Products = paginatedProducts,
                    CurrentPage = currentPage,
                    TotalPages = totalPages,
                    PageSize = pageSize,
                    SearchText = searchText,
                    CategoryFilter = categoryFilter,
                    SortOption = sortOption
                };

                return View("BuyerView", vm);
            }

            var ordersCount = _orderRepository.GetAllOrder().Where(o => o.OrderDate >= DateTime.Now.AddMonths(-1)).Count();
            var usersCount = UserManager.Users.Where(x => x.IsBuyer).Count();
            var sellerCount = UserManager.Users.Where(x => x.IsSeller).Count();
            var productsCount = _productRepository.GetAllProducts().Count();
            var paymentsCount = _paymentsRepository.GetAllPayments().Count();
            var totalSales = _paymentsRepository.GetAllPayments().Sum(p => p.Amount);

            List<MonthlySalesData> monthlySales = new List<MonthlySalesData>();
            if (User.IsInRole("Admin"))
            {
                monthlySales = _paymentsRepository.GetAllPayments()
                .Where(p => p.PaymentDate != null)
                .GroupBy(p => new { Year = p.PaymentDate.Value.Year, Month = p.PaymentDate.Value.Month })
                .Select(g => new MonthlySalesData
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:00}",
                    Sales = g.Sum(p => p.Amount)
                })
                .OrderBy(ms => ms.Month)
                .ToList() ?? new List<MonthlySalesData>();
            }
            else
            {
                var sellerId = Guid.Parse(UserManager.GetUserId(User));

                monthlySales = _orderRepository.GetAllOrder()
                    .Where(o => o.Payment != null && o.Payment.PaymentDate != null && o.OrderItems.Any(oi => oi.Product.SellerID == sellerId))
                    .Select(o => new
                    {
                        o.Payment.PaymentDate.Value.Year,
                        o.Payment.PaymentDate.Value.Month,
                        Amount = o.Payment.Amount,
                        SellerAmount = o.OrderItems.Where(oi => oi.Product.SellerID == sellerId).Sum(oi => oi.Quantity * oi.PricePerUnit)
                    })
                    .GroupBy(x => new { x.Year, x.Month })
                    .Select(g => new MonthlySalesData
                    {
                        Month = $"{g.Key.Year}-{g.Key.Month:00}",
                        Sales = g.Sum(x => x.SellerAmount)
                    })
                    .OrderBy(ms => ms.Month)
                    .ToList() ?? new List<MonthlySalesData>();
            }


            IEnumerable<Order> orders;
            if (User.IsInRole("Admin"))
            {
                orders = _orderRepository.GetAllOrder();
            }
            else
            {
                orders = _orderRepository.GetOrdersBySellerId(user.Id);
            }

            var topOrderedProducts = orders
           .Where(o => o.OrderDate >= DateTime.Now.AddMonths(-1))
           .SelectMany(o => o.OrderItems)
           .GroupBy(oi => oi.ProductId)
           .Select(g => new
           {
               ProductId = g.Key,
               OrderCount = g.Sum(oi => oi.Quantity)
           })
           .OrderByDescending(g => g.OrderCount)
           .Take(5)
           .Join(_productRepository.GetAllProducts(),
               orderGroup => orderGroup.ProductId,
               product => product.Id,
               (orderGroup, product) => new TopOrderedProductViewModel
               {
                   ProductId = product.Id,
                   ProductName = product.Title,
                   OrderCount = orderGroup.OrderCount,
                   ProductImage = product.ProductImage,
                   Price = product.Price,
               })
           .ToList();

            var dashBoardViewModel = new DashBoardViewModel()
            {
                OrdersCount = ordersCount,
                UsersCount = usersCount,
                PaymentsCount = paymentsCount,
                SellersCount = sellerCount,
                ProductsCount = productsCount,
                TotalSales = totalSales,
                MonthlySales = monthlySales,
                TopOrderedProducts = topOrderedProducts
            };

            return View(dashBoardViewModel);
        }

        public IActionResult Details(int id)
        {
            var product = _productRepository.GetProductById(id);
            return View(product);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
