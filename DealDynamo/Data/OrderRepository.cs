using DealDynamo.Models;
using DealDynamo.Services;
using Microsoft.EntityFrameworkCore;

namespace DealDynamo.Data
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DealDynamoContext _context;
        public OrderRepository(DealDynamoContext context)
        {
            _context = context;
        }
        public void AddOrder(Order order)
        {
            _context.Orders.Add(order);
            SaveChanges();
        }

        public void DeleteOrder(Order order)
        {
            _context.Orders.Remove(order);
            SaveChanges();
        }

        public IEnumerable<Order> GetAllOrder()
        {
            return _context.Orders.ToList();
        }

        public Order GetOrderById(int id)
        {
            return _context.Orders.Include(o => o.OrderItems)
                .Include(o => o.Payment)
                .Include(o => o.Buyer)
                .Include(o => o.Address).FirstOrDefault(x => x.Id == id);
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public void UpdateOrder(Order order)
        {
            _context.Orders.Update(order);
            SaveChanges();
        }
    }
}
