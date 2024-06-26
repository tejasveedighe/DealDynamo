﻿using DealDynamo.Areas.Identity.Data;
using DealDynamo.Models;
using DealDynamo.Models.ProductViewModels;
using DealDynamo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Drawing.Printing;
using System.Linq;

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
        public async Task<ActionResult> Index(int currentPage = 1, int pageSize = 4, int? categoryFilter = null, string sortOrder = "title_asc", string sellerFilter = "")
        {
            IEnumerable<Product> products = _productRepository.GetAllProducts();
            var sellers = UserManager.Users.Where(x => x.IsSeller).ToDictionary(x => x.Id, x => x.UserName);

            if (await UserManager.IsInRoleAsync(await UserManager.GetUserAsync(User), "Seller"))
            {
                string currentUserId = UserManager.GetUserId(User);
                Guid currentSellerId = Guid.Parse(currentUserId);
                products = products.Where(x => x.SellerID == currentSellerId);
            }

            // Apply category filter
            if (categoryFilter.HasValue)
            {
                products = products.Where(p => p.CategoryID == categoryFilter.Value);
            }

            // Apply sorting
            switch (sortOrder)
            {
                case "title_desc":
                    products = products.OrderByDescending(p => p.Title);
                    break;
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case "quantity_asc":
                    products = products.OrderBy(p => p.Quantity);
                    break;
                case "quantity_desc":
                    products = products.OrderByDescending(p => p.Quantity);
                    break;
                default:
                    products = products.OrderBy(p => p.Title);
                    break;
            }

            if (!string.IsNullOrEmpty(sellerFilter))
            {
                products = products.Where(x => x.SellerID == Guid.Parse(sellerFilter));
            }

            int totalProducts = products.Count();
            int totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            var paginatedProducts = products
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new ProductListViewModel
            {
                Products = paginatedProducts,
                CurrentPage = currentPage,
                TotalPages = totalPages,
                PageSize = pageSize,
                Sellers = sellers
            };

            ViewBag.CategoryFilter = categoryFilter;
            ViewBag.SortOrder = sortOrder;
            ViewBag.SellerFilter = sellerFilter;

            return View(viewModel);
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

        // GET: ProductController/Checkout
        [Authorize(Roles = "Seller")]
        public ActionResult Create()
        {
            CreateProductViewModel vm = new CreateProductViewModel()
            {
                Categories = _categoryRepository.GetAllCategories(),
            };
            return View(vm);
        }

        // POST: ProductController/Checkout
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
                    ProductImage = vm.NewProductImage != null ? UploadFile(vm.NewProductImage, vm.NewProductImage.FileName) : vm.ProductImage,
                    SellerID = Guid.Parse(UserManager.GetUserId(User)),
                };

                _productRepository.UpdateProduct(product);

                TempData["UpdateSuccess"] = true;

                return RedirectToAction(nameof(Edit), new { id = vm.ProductId });
            }
            catch
            {
                return View();
            }
        }

        // GET: ProductController/Delete/5
        [Authorize(Roles = "Seller, Admin")]
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
