using DealDynamo.Models;
using DealDynamo.Services;
using Microsoft.EntityFrameworkCore;

namespace DealDynamo.Data
{
    public class ProductReviewRepository : IProductReviewRepository
    {
        private readonly DealDynamoContext _context;

        public ProductReviewRepository(DealDynamoContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductReview>> GetAllReviewsAsync()
        {
            return await _context.ProductReviews.ToListAsync();
        }

        public async Task<ProductReview> GetReviewByIdAsync(int reviewId)
        {
            return await _context.ProductReviews.FindAsync(reviewId);
        }

        public async Task<IEnumerable<ProductReview>> GetReviewsByProductIdAsync(int productId)
        {
            return await _context.ProductReviews
                .Where(r => r.ProductID == productId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductReview>> GetReviewsByUserIdAsync(string userId)
        {
            return await _context.ProductReviews
                .Where(r => r.UserId == userId)
                .ToListAsync();
        }

        public async Task AddReviewAsync(ProductReview review)
        {
            _context.ProductReviews.Add(review);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateReviewAsync(ProductReview review)
        {
            _context.ProductReviews.Update(review);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteReviewAsync(int reviewId)
        {
            var review = await _context.ProductReviews.FindAsync(reviewId);
            if (review != null)
            {
                _context.ProductReviews.Remove(review);
                await _context.SaveChangesAsync();
            }
        }
    }
}
