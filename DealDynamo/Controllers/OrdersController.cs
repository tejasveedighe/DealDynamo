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
using System.Collections.Generic;
using System.Security.Claims;

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
        public async Task<IActionResult> Index(int currentPage = 1, int pageSize = 10)
        {
            var user = await _userManager.GetUserAsync(User);
            IEnumerable<Order> orders;
            if (User.IsInRole("Seller"))
            {
                orders = _orderRepository.GetOrdersBySellerId(user.Id);
            }
            else
            {
                orders = _orderRepository.GetAllOrder();
            }
            var totalOrders = orders.Count();
            var totalPages = (int)Math.Ceiling((double)totalOrders / 10);

            var paginatedOrders = orders.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

            var vm = new OrderListViewModel()
            {
                Orders = paginatedOrders,
                CurrentPage = currentPage,
                TotalPages = totalPages,
                PageSize = pageSize,
            };

            return View(vm);
        }

        // GET: OrdersController/Details/5
        [Authorize(Roles = "Admin, Seller")]
        public ActionResult Details(int id)
        {
            var order = _orderRepository.GetOrderById(id);
            return View(order);
        }

        [Authorize(Roles = "Buyer")]
        public async Task<IActionResult> Checkout()
        {
            try
            {
                var cart = HttpContext.Session.GetJson<List<AppCartItem>>("cart");
                var user = await _userManager.GetUserAsync(User);
                Order order = new Order()
                {
                    Buyer = user,
                    OrderDate = DateTime.Now,
                    OrderStatus = Models.Enums.OrderStatusEnum.Pending,
                    ShippingDate = null,
                    TotalPrice = cart.Sum(x => x.Product.Price * x.Quantity),
                    OrderItems = new List<OrderItems>(),
                    Address = new Models.Address() { City = "", Country = "", HouseNumber = "", PostalCode = "", Street = "" },
                };
                foreach (var item in cart)
                {
                    order.OrderItems.Add(new OrderItems()
                    {
                        PricePerUnit = item.Product.Price,
                        ProductId = item.Product.Id,
                        Quantity = item.Quantity,
                        SellerId = item.Product.SellerID.ToString(),
                    });
                }
                _orderRepository.AddOrder(order);

                var domain = "http://localhost:5035/";
                var options = new SessionCreateOptions()
                {
                    SuccessUrl = domain + $"Orders/OrderConfirmation?orderId={order.Id}&sessionId={{CHECKOUT_SESSION_ID}}",
                    CancelUrl = domain + $"Orders/Cancel?orderId={order.Id}&sessionId={{CHECKOUT_SESSION_ID}}",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    BillingAddressCollection = "auto",
                    ShippingAddressCollection = new SessionShippingAddressCollectionOptions()
                    {
                        AllowedCountries = new List<string> { "US" }
                    },
                    CustomerEmail = user.Email,
                };

                foreach (var item in cart)
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
                                Description = item.Product.Description,
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
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            var order = _orderRepository.GetOrderById(id);
            return View(order);
        }

        // POST: OrdersController/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Order order)
        {
            try
            {
                var dbOrder = _orderRepository.GetOrderById(order.Id);
                dbOrder.OrderStatus = order.OrderStatus;
                dbOrder.ShippingDate = order.ShippingDate;
                _orderRepository.UpdateOrder(dbOrder);

                return RedirectToAction(nameof(Edit));
            }
            catch
            {
                return RedirectToAction(nameof(Edit));
            }
        }

        // GET: OrdersController/Delete/5
        public ActionResult Delete(int id)
        {
            var order = _orderRepository.GetOrderById(id);
            return View(order);
        }

        // POST: OrdersController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                var order = _orderRepository.GetOrderById(id);
                _orderRepository.DeleteOrder(order);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public IActionResult OrderConfirmation(int orderId, string sessionId)
        {
            var sessionService = new Stripe.Checkout.SessionService();
            var session = sessionService.Get(sessionId);

            string userId = _userManager.GetUserId(User);
            _cartRepository.ClearCart(userId);

            var order = _orderRepository.GetOrderById(orderId);
            if (order == null) return Problem("Order Not Found, please try later");

            order.Address.Country = session.ShippingDetails.Address.Country;
            order.Address.Street = session.ShippingDetails.Address.Line2;
            order.Address.City = session.ShippingDetails.Address.City;
            order.Address.HouseNumber = session.ShippingDetails.Address.Line1;
            order.Address.PostalCode = session.ShippingDetails.Address.PostalCode;

            order.Payment = new Payments()
            {
                Amount = order.TotalPrice,
                Order = order,
                OrderId = order.Id,
                PaymentDate = DateTime.Now,
                Status = session.PaymentStatus == "paid" ? Models.Enums.PaymentStatusEnum.Complete : Models.Enums.PaymentStatusEnum.Failed,
                StripePaymentId = session.PaymentIntentId,
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

        public IActionResult Cancel(int orderId, string sessionId)
        {
            var sessionService = new Stripe.Checkout.SessionService();
            var session = sessionService.Get(sessionId);

            var order = _orderRepository.GetOrderById(orderId);

            order.OrderStatus = Models.Enums.OrderStatusEnum.Cancelled;

            order.Payment = new Payments()
            {
                Amount = order.TotalPrice,
                Order = order,
                OrderId = order.Id,
                PaymentDate = DateTime.Now,
                Status = Models.Enums.PaymentStatusEnum.Failed,
                StripePaymentId = null,
            };
            _orderRepository.UpdateOrder(order);
            return View(order);
        }

        [Authorize(Roles = "Buyer")]
        public IActionResult ViewOrders(int currentPage = 1, int pageSize = 10)
        {
            var userId = _userManager.GetUserId(User);
            var orders = _orderRepository.GetAllOrder().Where(x => x.BuyerId == userId).ToList();

            var totalOrders = orders.Count();
            var totalPages = (int)Math.Ceiling((double)totalOrders / 10);

            var paginatedOrders = orders.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

            var vm = new OrderListViewModel()
            {
                Orders = paginatedOrders,
                CurrentPage = currentPage,
                TotalPages = totalPages,
                PageSize = pageSize,
            };

            return View(vm);
        }

        public IActionResult OrderDetails(int id)
        {
            var order = _orderRepository.GetOrderById(id);
            return View(order);
        }
        public IActionResult CancelOrder(int id)
        {
            var order = _orderRepository.GetOrderById(id);
            order.OrderStatus = Models.Enums.OrderStatusEnum.Cancelled;
            order.Payment.Status = Models.Enums.PaymentStatusEnum.Refunded;
            _orderRepository.UpdateOrder(order);

            return RedirectToAction(nameof(OrderDetails), new { id });
        }
    }
}
