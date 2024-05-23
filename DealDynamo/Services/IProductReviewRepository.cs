using DealDynamo.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace DealDynamo.Services
{
    public interface IProductReviewRepository
    {
        Task<IEnumerable<ProductReview>> GetAllReviewsAsync();
        Task<ProductReview> GetReviewByIdAsync(int reviewId);
        Task<IEnumerable<ProductReview>> GetReviewsByProductIdAsync(int productId);
        Task<IEnumerable<ProductReview>> GetReviewsByUserIdAsync(string userId);
        Task AddReviewAsync(ProductReview review);
        Task UpdateReviewAsync(ProductReview review);
        Task DeleteReviewAsync(int reviewId);
    }
}
