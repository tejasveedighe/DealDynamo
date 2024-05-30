using DealDynamo.Models;
using DealDynamo.Services;

namespace DealDynamo.Data
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly DealDynamoContext _context;
        public OrderItemRepository(DealDynamoContext context)
        {
            _context = context;
        }

        public void DeleteItem(OrderItems item)
        {
            _context.OrderItems.Remove(item);
            SaveChanges();
        }

        public IQueryable<OrderItems> GetAllItemByOrderId(int orderId)
        {
           return _context.OrderItems.Where(oi => oi.OrderId == orderId);
        }

        public IQueryable<OrderItems> GetAllItems()
        {
            return _context.OrderItems;
        }

        public OrderItems GetItemById(int id)
        {
            return _context.OrderItems.FirstOrDefault(oi => oi.Id == id);
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public void UpdateItem(OrderItems item)
        {
            _context.OrderItems.Update(item);
            SaveChanges();
        }
    }
}
