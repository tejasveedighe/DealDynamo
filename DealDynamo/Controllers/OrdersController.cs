using DealDynamo.Areas.Identity.Data;
using DealDynamo.Helper;
using DealDynamo.Models;
using DealDynamo.Models.OrderViewModels;
using DealDynamo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace DealDynamo.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICartRepository _cartRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        public OrdersController(IOrderRepository orderRepository, UserManager<ApplicationUser> userManager, ICartRepository cartRepository, IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _userManager = userManager;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
        }
        // GET: OrdersController
        [Authorize(Roles = "Admin, Seller")]
        public ActionResult Index()
        {
            var orders = _orderRepository.GetAllOrder();
            return View(orders);
        }

        // GET: OrdersController/Details/5
        [Authorize(Roles = "Admin, Seller")]
        public ActionResult Details(int id)
        {
            var order = _orderRepository.GetOrderById(id);
            return View(order);
        }

        // GET: OrdersController/Checkout
        public ActionResult Checkout()
        {
            var cart = HttpContext.Session.GetJson<List<AppCartItem>>("cart");
            OrderCheckoutViewModel vm = new OrderCheckoutViewModel()
            {
                CartItems = cart,
                Address = new Models.Address(),
            };
            return View(vm);
        }

        // POST: OrdersController/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(OrderCheckoutViewModel vm)
        {
            try
            {
                Order order = new Order()
                {
                    BuyerId = Guid.Parse(_userManager.GetUserId(User)),
                    Address = vm.Address,
                    OrderDate = DateTime.Now,
                    OrderStatus = Models.Enums.OrderStatusEnum.Pending,
                    ShippingDate = null,
                    TotalPrice = vm.CartItems.Sum(x => x.Product.Price * x.Quantity),
                    OrderItems = new List<OrderItems>(),
                };
                foreach (AppCartItem item in vm.CartItems)
                {
                    order.OrderItems.Add(new OrderItems()
                    {
                        PricePerUnit = item.Product.Price,
                        ProductId = item.Product.Id,
                        Quantity = item.Quantity,
                    }
                    );
                }
                _orderRepository.AddOrder(order);

                var domain = "http://localhost:5035/";
                var options = new SessionCreateOptions()
                {
                    SuccessUrl = domain + $"Orders/OrderConfirmation?orderId={order.Id}&sessionUrl=",
                    CancelUrl = domain + "Orders/Cancel",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    BillingAddressCollection = "auto",
                    ShippingAddressCollection = new SessionShippingAddressCollectionOptions()
                    {
                        AllowedCountries = new List<string> { "US" }
                    },
                };

                foreach (var item in vm.CartItems)
                {
                    var sessionListItem = new SessionLineItemOptions()
                    {
                        PriceData = new SessionLineItemPriceDataOptions()
                        {
                            UnitAmount = (long)(item.Product.Price * item.Quantity),
                            Currency = "USD",
                            ProductData = new SessionLineItemPriceDataProductDataOptions()
                            {
                                Name = item.Product.Title,
                            }
                        },
                        Quantity = item.Quantity
                    };
                    options.LineItems.Add(sessionListItem);
                }

                var service = new SessionService();
                Session session = service.Create(options);

                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }
            catch
            {
                return RedirectToAction(nameof(Checkout));
            }
        }

        // GET: OrdersController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: OrdersController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: OrdersController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: OrdersController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public IActionResult OrderConfirmation(int orderId)
        {
            string userId = _userManager.GetUserId(User);
            _cartRepository.ClearCart(userId);

            var order = _orderRepository.GetOrderById(orderId);
            order.Payment = new Payments()
            {
                Amount = order.TotalPrice,
                Order = order,
                OrderId = order.Id,
                PaymentDate = DateTime.Now,
                Status = Models.Enums.PaymentStatusEnum.Complete,
            };
            _orderRepository.UpdateOrder(order);

            foreach (var item in order.OrderItems)
            {
                var product = item.Product;
                product.Quantity -= item.Quantity;
                _productRepository.UpdateProduct(product);
            }

            return View(order);
        }
    }
}
