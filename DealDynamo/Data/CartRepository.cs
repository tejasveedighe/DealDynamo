using DealDynamo.Areas.Identity.Data;
using DealDynamo.Models;
using DealDynamo.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DealDynamo.Data
{
    public class CartRepository : ICartRepository
    {
        private readonly DealDynamoContext _db;
        public CartRepository(DealDynamoContext db)
        {
            _db = db;
        }
        public void AddCartItem(string userId, AppCartItem item)
        {
            _db.CartItems.Add(new CartItem()
            {
                ProductId = item.Product.Id,
                Quantity = item.Quantity,
                UserId = Guid.Parse(userId),
            });
            SaveChanges();
        }

        public void ClearCart(string userId)
        {
            _db.CartItems.RemoveRange(_db.CartItems.Where(x => x.UserId == Guid.Parse(userId)));
            SaveChanges();
        }

        public void DecreaseQuantity(string userId, AppCartItem item)
        {
 var existingCartItem = _db.CartItems.SingleOrDefault(ci => ci.Id == item.Id && ci.UserId == Guid.Parse(userId));

            if (existingCartItem != null)
            {
                // Update the quantity directly
                existingCartItem.Quantity--;
                _db.CartItems.Update(existingCartItem);
            }

            SaveChanges();
        }

        public List<AppCartItem> GetCartItems(string userId)
        {
            var items = new List<AppCartItem>();
            var dbItems = _db.CartItems.Where(x => x.UserId == Guid.Parse(userId)).Include(x => x.Product);
            if (!dbItems.Any()) return items;

            foreach (var item in dbItems.ToList())
            {
                items.Add(new AppCartItem()
                {
                    Id = item.Id,
                    Product = item.Product,
                    Quantity = item.Quantity,
                });

            }
            return items;
        }

        public void IncreaseQuantity(string userId, AppCartItem item)
        {
           var existingCartItem = _db.CartItems.SingleOrDefault(ci => ci.Id == item.Id && ci.UserId == Guid.Parse(userId));

            if (existingCartItem != null)
            {
                // Update the quantity directly
                existingCartItem.Quantity++;
                _db.CartItems.Update(existingCartItem);
            }
            SaveChanges();
        }

        public void RemoveCartItem(string userId, AppCartItem item)
        {
            _db.CartItems.Remove(new CartItem()
            {
                Id = item.Id,
                ProductId = item.Product.Id,
                Quantity = item.Quantity,
                UserId = Guid.Parse(userId),
            });
            SaveChanges();
        }

        public void SaveChanges()
        {
            _db.SaveChanges();
        }
    }
}
