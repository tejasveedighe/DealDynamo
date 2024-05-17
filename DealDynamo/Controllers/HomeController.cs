using DealDynamo.Areas.Identity.Data;
using DealDynamo.Models;
using DealDynamo.Models.ProductViewModels;
using DealDynamo.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DealDynamo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> UserManager;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _logger = logger;
            UserManager = userManager;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index(int currentPage = 1, int pageSize = 10)
        {
            var user = await UserManager.GetUserAsync(User);

            var categories = _categoryRepository.GetAllCategories().ToList();
            ViewBag.Categories = categories;

            if (!User.Identity.IsAuthenticated || (user != null && user.IsBuyer))
            {
                var products = _productRepository.GetAllProducts();
                var totalProducts = products.Count();
                var totalPages = (int)Math.Ceiling((double)totalProducts / 10);

                var paginatedProducts = products.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

                var vm = new ProductCardListViewModel()
                {
                    Products = paginatedProducts,
                    CurrentPage = currentPage,
                    TotalPages = totalPages,
                    PageSize = pageSize,
                };

                return View("BuyerView", vm);
            }
            return View();
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
