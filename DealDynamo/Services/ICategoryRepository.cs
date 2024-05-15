using DealDynamo.Models;

namespace DealDynamo.Services
{
    public interface ICategoryRepository
    {
        public IEnumerable<Category> GetAllCategories();
        public Category GetCategoryById(int? id);
        public void AddCategory(Category category);
        public void UpdateCategory(Category category);
        public void DeleteCategory(int? id);
        public void SaveChanges();
    }
}
