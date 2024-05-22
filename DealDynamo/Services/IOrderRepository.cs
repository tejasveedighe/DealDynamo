using DealDynamo.Models;

namespace DealDynamo.Services
{
    public interface IOrderRepository
    {
        IEnumerable<Order> GetAllOrder();
        IEnumerable<Order> GetOrdersBySellerId(string sellerId);
        Order GetOrderById(int id);
        void DeleteOrder(Order order);
        void UpdateOrder(Order order);
        void AddOrder(Order order);
        void SaveChanges();
    }
}
