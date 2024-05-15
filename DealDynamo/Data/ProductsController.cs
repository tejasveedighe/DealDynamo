using DealDynamo.Areas.Identity.Data;
using DealDynamo.Models;
using DealDynamo.Models.ProductViewModels;
using DealDynamo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DealDynamo.Data
{
    [Authorize(Roles = "Admin, Seller")]
    public class ProductsController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly UserManager<ApplicationUser> UserManager;

        public ProductsController(IProductRepository productRepository, ICategoryRepository categoryRepository, UserManager<ApplicationUser> userManager)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            UserManager = userManager;
        }
        // GET: ProductController
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            IEnumerable<Product> products = _productRepository.GetAllCategories();

            string currentUserId = UserManager.GetUserId(User);
            Guid currentSellerId = Guid.Parse(currentUserId); // Parse string to Guid

            if (!await UserManager.IsInRoleAsync(await UserManager.GetUserAsync(User), "Admin"))
            {
                products = products.Where(x => x.SellerID == currentSellerId);
            }

            return View(products);
        }

        // GET: ProductController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ProductController/Create
        [Authorize(Roles = "Seller")]
        public ActionResult Create()
        {
            CreateProductViewModel vm = new CreateProductViewModel()
            {
                Category = new SelectList(_categoryRepository.GetAllCategories(), "Id", "Title")
            };
            return View(vm);
        }

        // POST: ProductController/Create
        [Authorize(Roles = "Seller")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateProductViewModel product)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ProductController/Edit/5
        [Authorize(Roles = "Seller")]
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ProductController/Edit/5
        [Authorize(Roles = "Seller")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ProductController/Delete/5
        [Authorize(Roles = "Seller")]
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ProductController/Delete/5
        [Authorize(Roles = "Seller")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
