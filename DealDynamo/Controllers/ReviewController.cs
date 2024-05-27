using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DealDynamo.Data;
using DealDynamo.Models;
using DealDynamo.Services;
using Microsoft.AspNetCore.Identity;
using DealDynamo.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using DealDynamo.Models.ReviewViewModels;

namespace DealDynamo.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly DealDynamoContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProductReviewRepository _productReviewRepository;
        private readonly IProductRepository _productRepository;

        public ReviewController(DealDynamoContext context, UserManager<ApplicationUser> userManager, IProductReviewRepository productReviewRepository, IProductRepository productRepository)
        {
            _context = context;
            _userManager = userManager;
            _productReviewRepository = productReviewRepository;
            _productRepository = productRepository;
        }

        // GET: Review
        [Authorize(Roles = "Admin, Seller")]
        public async Task<IActionResult> Index(string buyerNameFilter, string productNameFilter, int? ratingFilter, DateTime? dateSubmittedFilter, string sortBy, bool isSortAscending = true, int currentPage = 1, int pageSize = 10)
        {
            var user = await _userManager.GetUserAsync(User);

            // Get the user's roles
            var roles = await _userManager.GetRolesAsync(user);

            IQueryable<ProductReview> reviewsQuery = _context.ProductReviews
                .Include(p => p.Product)
                .Include(p => p.User);

            // If the user is a seller, filter the reviews
            if (roles.Contains("Seller"))
            {
                var currentUserId = user.Id;
                reviewsQuery = reviewsQuery.Where(r => r.Product.SellerID == Guid.Parse(currentUserId));
            }

            // Apply filters
            if (!string.IsNullOrEmpty(buyerNameFilter))
            {
                reviewsQuery = reviewsQuery.Where(r => r.User.UserName.Contains(buyerNameFilter));
            }

            if (!string.IsNullOrEmpty(productNameFilter))
            {
                reviewsQuery = reviewsQuery.Where(r => r.Product.Title.Contains(productNameFilter));
            }

            if (ratingFilter.HasValue)
            {
                reviewsQuery = reviewsQuery.Where(r => r.Rating == ratingFilter.Value);
            }

            if (dateSubmittedFilter.HasValue)
            {
                reviewsQuery = reviewsQuery.Where(r => r.DateSubmitted.Date == dateSubmittedFilter.Value.Date);
            }

            // Apply sorting
            reviewsQuery = sortBy switch
            {
                "buyer" => isSortAscending ? reviewsQuery.OrderBy(r => r.User.UserName) : reviewsQuery.OrderByDescending(r => r.User.UserName),
                "product" => isSortAscending ? reviewsQuery.OrderBy(r => r.Product.Title) : reviewsQuery.OrderByDescending(r => r.Product.Title),
                "date" => isSortAscending ? reviewsQuery.OrderBy(r => r.DateSubmitted) : reviewsQuery.OrderByDescending(r => r.DateSubmitted),
                _ => reviewsQuery.OrderBy(r => r.DateSubmitted)
            };

            // Pagination
            var totalReviews = await reviewsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalReviews / pageSize);
            var reviews = await reviewsQuery.Skip((currentPage - 1) * pageSize).Take(pageSize).ToListAsync();

            var viewModel = new ReviewListViewModel
            {
                Reviews = reviews,
                BuyerNameFilter = buyerNameFilter,
                ProductNameFilter = productNameFilter,
                RatingFilter = ratingFilter,
                DateSubmittedFilter = dateSubmittedFilter,
                SortBy = sortBy,
                IsSortAscending = isSortAscending,
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        // GET: Review/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ProductReviews == null)
            {
                return NotFound();
            }

            var productReview = await _context.ProductReviews
                .Include(p => p.Product)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (productReview == null)
            {
                return NotFound();
            }

            return View(productReview);
        }

        // GET: Review/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.ProductReviews == null)
            {
                return NotFound();
            }

            var productReview = await _context.ProductReviews.FindAsync(id);
            if (productReview == null)
            {
                return NotFound();
            }
            ViewData["ProductID"] = new SelectList(_context.Product, "Id", "Id", productReview.ProductID);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", productReview.UserId);

            productReview.DateSubmitted = DateTime.Now;

            return View(productReview);
        }

        // POST: Review/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,ProductID,UserId,Rating,Comment,DateSubmitted")] ProductReview productReview)
        {
            if (id != productReview.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(productReview);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductReviewExists(productReview.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductID"] = new SelectList(_context.Product, "Id", "Id", productReview.ProductID);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", productReview.UserId);


            return RedirectToAction(actionName: "Details", controllerName: "Home", routeValues: new { id = productReview.ProductID });
        }

        // GET: Review/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.ProductReviews == null)
            {
                return NotFound();
            }

            var productReview = await _context.ProductReviews
                .Include(p => p.Product)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (productReview == null)
            {
                return NotFound();
            }

            return View(productReview);
        }

        // POST: Review/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ProductReviews == null)
            {
                return Problem("Entity set 'DealDynamoContext.ProductReviews'  is null.");
            }
            var productReview = await _context.ProductReviews.FindAsync(id);
            if (productReview != null)
            {
                _context.ProductReviews.Remove(productReview);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductReviewExists(int id)
        {
            return (_context.ProductReviews?.Any(e => e.ID == id)).GetValueOrDefault();
        }

        [Authorize(Roles = "Buyer")]
        public async Task<IActionResult> AddReview(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            var reviews = new ProductReview()
            {
                ProductID = productId,
                Product = _productRepository.GetProductById(productId),
                User = user,
                UserId = user.Id,
                DateSubmitted = DateTime.Now,
            };
            return View(reviews);
        }

        [Authorize(Roles = "Buyer")]
        [HttpPost]
        public async Task<IActionResult> AddReview(ProductReview review)
        {
            await _productReviewRepository.AddReviewAsync(review);
            return RedirectToAction(actionName: "Details", controllerName: "Home", routeValues: new { id = review.ProductID });
        }
    }
}
