using DealDynamo.Models;
using DealDynamo.Services;

namespace DealDynamo.Data
{
    public class ProductRepository : IProductRepository
    {
        private DealDynamoContext _db;
        public ProductRepository(DealDynamoContext db)
        {
            this._db = db;
        }
        private bool ProductExists(int? id)
        {
            var product = GetProductById(id);
            if (product == null) return false;
            return true;
        }

        public void AddProduct(Product product)
        {
            _db.Product.Add(product);
        }

        public void DeleteProduct(int? id)
        {
            if (ProductExists(id))
            {
                _db.Product.Remove(GetProductById(id));
            }
        }

        public IEnumerable<Product> GetAllCategories()
        {
            return _db.Product.ToList();
        }

        public Product GetProductById(int? id)
        {
            return _db.Product.SingleOrDefault(x => x.Id == id);
        }

        public void SaveChanges()
        {
            _db.SaveChanges();
        }

        public void UpdateProduct(Product category)
        {
            _db.Product.Update(category);
        }
    }
}
