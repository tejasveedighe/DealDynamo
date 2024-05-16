using DealDynamo.Areas.Identity.Data;
using DealDynamo.Models;
using DealDynamo.Models.ProductViewModels;
using DealDynamo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DealDynamo.Controllers
{
    [Authorize(Roles = "Admin, Seller")]
    public class ProductsController : Controller
    {
        private readonly IWebHostEnvironment WebHostEnvironment;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly UserManager<ApplicationUser> UserManager;

        public ProductsController(IProductRepository productRepository,
                                  ICategoryRepository categoryRepository,
                                  UserManager<ApplicationUser> userManager,
                                  IWebHostEnvironment webHostEnvironment)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            UserManager = userManager;
            WebHostEnvironment = webHostEnvironment;
        }
        // GET: ProductController
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            IEnumerable<Product> products = _productRepository.GetAllCategories();

            if (!await UserManager.IsInRoleAsync(await UserManager.GetUserAsync(User), "Seller"))
            {
                string currentUserId = UserManager.GetUserId(User);
                Guid currentSellerId = Guid.Parse(currentUserId); // Parse string to Guid
                products = products.Where(x => x.SellerID == currentSellerId);
            }

            return View(products);
        }

        // GET: ProductController/Details/5
        public ActionResult Details(int id)
        {
            var product = _productRepository.GetProductById(id);
            ViewDetailProductViewModel vm = new ViewDetailProductViewModel()
            {
                ProductId = product.Id,
                Title = product.Title,
                Quantity = product.Quantity,
                Price = product.Price,
                Description = product.Description,
                ProductImage = product.ProductImage,
                Category = _categoryRepository.GetCategoryById(product.CategoryID)
            };
            return View(vm);
        }

        // GET: ProductController/Create
        [Authorize(Roles = "Seller")]
        public ActionResult Create()
        {
            CreateProductViewModel vm = new CreateProductViewModel()
            {
                Categories = _categoryRepository.GetAllCategories(),
            };
            return View(vm);
        }

        // POST: ProductController/Create
        [Authorize(Roles = "Seller")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateProductViewModel vm)
        {
            try
            {
                Product product = new Product()
                {
                    Title = vm.Title,
                    Quantity = vm.Quantity,
                    Description = vm.Description,
                    CategoryID = vm.CategoryId,
                    SellerID = Guid.Parse(UserManager.GetUserId(User)),
                    Price = vm.Price,
                };

                product.ProductImage = UploadFile(vm.ProductImage, vm.ProductImage.FileName);

                _productRepository.AddProduct(product);
                _productRepository.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Some Error Occured, please try later");
                return View();
            }
        }

        private string UploadFile(IFormFile file, string fileName)
        {
            string uploadsFolder = Path.Combine(WebHostEnvironment.WebRootPath, "images");
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            return uniqueFileName;
        }

        // GET: ProductController/Edit/5
        [Authorize(Roles = "Seller")]
        public ActionResult Edit(int id)
        {
            var product = _productRepository.GetProductById(id);
            EditProductViewModel vm = new EditProductViewModel()
            {
                ProductId = product.Id,
                Title = product.Title,
                Price = product.Price,
                Quantity = product.Quantity,
                ProductImage = product.ProductImage,
                Categories = _categoryRepository.GetAllCategories(),
                CategoryId = product.CategoryID,
                Description = product.Description,
            };
            return View(vm);
        }

        // POST: ProductController/Edit/5
        [Authorize(Roles = "Seller")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditProductViewModel vm)
        {
            try
            {
                Product product = new Product()
                {
                    Id = vm.ProductId,
                    Title = vm.Title,
                    Price = vm.Price,
                    Quantity = vm.Quantity,
                    CategoryID = vm.CategoryId,
                    Description = vm.Description,
                    ProductImage = UploadFile(vm.NewProductImage, vm.NewProductImage.FileName),
                    SellerID = Guid.Parse(UserManager.GetUserId(User)),
                };

                _productRepository.UpdateProduct(product);
                _productRepository.SaveChanges();

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
            Product product = _productRepository.GetProductById(id);
            return View(product);
        }

        // POST: ProductController/Delete/5
        [Authorize(Roles = "Seller")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                _productRepository.DeleteProduct(id);
                _productRepository.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
