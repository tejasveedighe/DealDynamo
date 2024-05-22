using DealDynamo.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DealDynamo.Models.OrderViewModels
{
    public class OrderEditViewModel
    {
        public OrderEditViewModel()
        {

        }
        public OrderEditViewModel(Order order)
        {
            OrderStatus = new List<SelectListItem>()
            {
                new SelectListItem()
                {
                    Text = OrderStatusEnum.Cancelled.ToString(),
                    Value = Convert.ToString(OrderStatusEnum.Cancelled)
                },
                new SelectListItem()
                {
                    Text = OrderStatusEnum.Pending.ToString(),
                    Value = Convert.ToString(OrderStatusEnum.Pending)
                },
                new SelectListItem()
                {
                    Text = OrderStatusEnum.Dispatched.ToString(),
                    Value = Convert.ToString(OrderStatusEnum.Dispatched)
                },
                new SelectListItem()
                {
                    Text = OrderStatusEnum.Compelete.ToString(),
                    Value = Convert.ToString(OrderStatusEnum.Compelete)
                }
            };

            foreach (var item in OrderStatus)
            {
                if (item.Value == order.OrderStatus.ToString())
                {
                    item.Selected = true;
                    break;
                }
            }

            Order = order;
        }
        public Order Order { get; set; }
        public List<SelectListItem> OrderStatus { get; set; }
    }
}
