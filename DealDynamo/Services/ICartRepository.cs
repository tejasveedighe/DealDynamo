using DealDynamo.Models;

namespace DealDynamo.Services
{
    public interface ICartRepository
    {
        List<AppCartItem> GetCartItems(string userId);
        void ClearCart(string userId);
        void AddCartItem(string userId, AppCartItem item);
        void RemoveCartItem(string userId, AppCartItem item);
        void DecreaseQuantity(string userId, AppCartItem item);
        void IncreaseQuantity(string userId, AppCartItem item);
        void SaveChanges();
    }
}
