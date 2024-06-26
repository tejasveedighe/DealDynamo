﻿using DealDynamo.Models;

namespace DealDynamo.Services
{
    public interface IOrderItemRepository
    {
        IQueryable<OrderItems> GetAllItems();
        IQueryable<OrderItems> GetAllItemByOrderId(int orderId);
        OrderItems GetItemById(int id);

        void UpdateItem(OrderItems item);
        void DeleteItem(OrderItems item);
        void SaveChanges();
    }
}
