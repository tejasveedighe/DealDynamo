using DealDynamo.Models;

namespace DealDynamo.Services
{
    public interface IProductRepository
    {
        public IEnumerable<Product> GetAllCategories();
        public Product GetProductById(int? id);
        public void AddProduct(Product product);
        public void UpdateProduct(Product product);
        public void DeleteProduct(int? id);
        public void SaveChanges();

    }
}
