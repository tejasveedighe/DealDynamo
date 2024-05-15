using DealDynamo.Models;
using DealDynamo.Services;

namespace DealDynamo.Data
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DealDynamoContext _db;

        public CategoryRepository(DealDynamoContext db)
        {
            _db = db;
        }
        private bool CategoryExists(int? id)
        {
            var category = _db.Category.Where(x => x.Id == id);
            if (category == null) return false;
            return true;
        }
        public void DeleteCategory(int? id)
        {
            if (CategoryExists(id))
            {
                _db.Category.Remove(GetCategoryById(id));
                SaveChanges();
            }
        }

        public IEnumerable<Category> GetAllCategories()
        {
            return _db.Category.ToList();
        }

        public Category GetCategoryById(int? id)
        {
            return _db.Category.SingleOrDefault(x => x.Id == id);
        }

        public void SaveChanges()
        {
            _db.SaveChanges();
        }

        public void UpdateCategory(Category category)
        {
            _db.Category.Update(category);
            SaveChanges();
        }

        public void AddCategory(Category category)
        {
            _db.Category.Add(category);
            SaveChanges();
        }
    }
}
