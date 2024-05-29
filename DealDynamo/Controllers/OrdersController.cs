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
        private readonly IEmailService _emailService;
        private readonly UserManager<ApplicationUser> _userManager;
        public OrdersController(IOrderRepository orderRepository, UserManager<ApplicationUser> userManager, ICartRepository cartRepository, IProductRepository productRepository, IEmailService emailService)
        {
            _orderRepository = orderRepository;
            _userManager = userManager;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _emailService = emailService;
        }

        // GET: OrdersController
        [Authorize(Roles = "Admin, Seller")]
        public async Task<IActionResult> Index(int currentPage = 1, int pageSize = 10, string orderStatusFilter = "All", string paymentStatusFilter = "All", string sortOrderDate = "asc")
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

            // Apply filters
            if (orderStatusFilter != "All")
            {
                orders = orders.Where(o => o.OrderStatus.ToString() == orderStatusFilter);
            }
            if (paymentStatusFilter != "All")
            {
                orders = orders.Where(o => o.Payment?.Status.ToString() == paymentStatusFilter);
            }

            // Apply sorting
            orders = sortOrderDate == "asc" ? orders.OrderBy(o => o.OrderDate) : orders.OrderByDescending(o => o.OrderDate);

            var totalOrders = orders.Count();
            var totalPages = (int)Math.Ceiling((double)totalOrders / pageSize);

            var paginatedOrders = orders.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

            var vm = new OrderListViewModel()
            {
                Orders = paginatedOrders,
                CurrentPage = currentPage,
                TotalPages = totalPages,
                PageSize = pageSize,
            };

            ViewBag.OrderStatusFilter = orderStatusFilter;
            ViewBag.PaymentStatusFilter = paymentStatusFilter;
            ViewBag.SortOrderDate = sortOrderDate;

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
                            UnitAmount = (long)(item.Product.Price * item.Quantity) * 10,
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
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var order = _orderRepository.GetOrderById(id);
            return View(order);
        }

        // POST: OrdersController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, IFormCollection collection)
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

        public async Task<IActionResult> OrderConfirmation(int orderId, string sessionId)
        {
            var sessionService = new Stripe.Checkout.SessionService();
            var session = sessionService.Get(sessionId);

            var user = await _userManager.GetUserAsync(User);
            _cartRepository.ClearCart(user.Id);

            HttpContext.Session.Clear();

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

            _emailService.SendEmail(new EmailData()
            {
                EmailToId = session.CustomerEmail,
                EmailToName = user.UserName,
                EmailSubject = "Order Confirmed",
                EmailBody = $@"
    <!DOCTYPE html>
    <html>
    <head>
        <title>Order Confirmation</title>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <style>
            body {{
                font-family: Arial, sans-serif;
                margin: 0;
                padding: 0;
            }}
            .container {{
                max-width: 600px;
                margin: 40px auto;
                padding: 20px;
                background-color: #f9f9f9;
                border: 1px solid #ddd;
                border-radius: 10px;
                box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
            }}
            .header {{
                background-color: #333;
                color: #fff;
                padding: 20px;
                border-bottom: 1px solid #333;
            }}
            .header h1 {{
                margin-top: 0;
            }}
            .content {{
                padding: 20px;
            }}
            .footer {{
                background-color: #333;
                color: #fff;
                padding: 10px;
                border-top: 1px solid #333;
            }}
            .order-items {{
                margin-top: 20px;
            }}
            .order-items th, .order-items td {{
                padding: 10px;
                border: 1px solid #ddd;
            }}
            .order-items th {{
                background-color: #f1f1f1;
            }}
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='header'>
                <h1>Order Confirmation</h1>
            </div>
            <div class='content'>
                <p>Dear {session.ShippingDetails.Name},</p>
                <p>Thank you for your recent order! We have received your payment and are processing your order.</p>
                <p>Your order details are as follows:</p>
                <ul>
                    <li>Order Number: {order.Id}</li>
                    <li>Order Date: {order.OrderDate?.ToString("yyyy-MM-dd")}</li>
                    <li>Total: {order.TotalPrice:C}</li>
                    <li>Order Status: {order.OrderStatus}</li>
                    <li>Payment Status: {order.Payment.Status}</li>
                </ul>
                <div class='order-items'>
                    <h2>Order Items</h2>
                    <table>
                        <thead>
                            <tr>
                                <th>Product</th>
                                <th>Quantity</th>
                                <th>Price Per Unit</th>
                                <th>Total</th>
                            </tr>
                        </thead>
                        <tbody>
                            {string.Join("", order.OrderItems.Select(item => $@"
                            <tr>
                                <td>{item.Product.Title}</td>
                                <td>{item.Quantity}</td>
                                <td>{item.PricePerUnit:C}</td>
                                <td>{item.Quantity * item.PricePerUnit:C}</td>
                            </tr>"))}
                        </tbody>
                    </table>
                </div>
                <p>We will send you an email when your order ships. If you have any questions or concerns, please don't hesitate to contact us.</p>
            </div>
            <div class='footer'>
                <p>&copy; Deal Dynamo {DateTime.Now.Year}</p>
            </div>
        </div>
    </body>
    </html>",
            });

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

            session.ExpiresAt = DateTime.Now;

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

        [HttpGet]
        [Authorize(Roles = "Buyer")]
        public IActionResult ViewOrders(int currentPage = 1, int pageSize = 10, string orderStatusFilter = "All", string paymentStatusFilter = "All", string sortOrderDate = "asc")
        {
            var userId = _userManager.GetUserId(User);
            var orders = _orderRepository.GetAllOrder().Where(x => x.BuyerId == userId).ToList();

            // Apply filters
            if (orderStatusFilter != "All")
            {
                orders = orders.Where(o => o.OrderStatus.ToString() == orderStatusFilter).ToList();
            }
            if (paymentStatusFilter != "All")
            {
                orders = orders.Where(o => o.Payment?.Status.ToString() == paymentStatusFilter).ToList();
            }

            // Apply sorting
            orders = sortOrderDate == "asc" ? orders.OrderBy(o => o.OrderDate).ToList() : orders.OrderByDescending(o => o.OrderDate).ToList();

            var totalOrders = orders.Count();
            var totalPages = (int)Math.Ceiling((double)totalOrders / pageSize);

            var paginatedOrders = orders.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

            var vm = new OrderListViewModel()
            {
                Orders = paginatedOrders,
                CurrentPage = currentPage,
                TotalPages = totalPages,
                PageSize = pageSize,
            };

            ViewBag.OrderStatusFilter = orderStatusFilter;
            ViewBag.PaymentStatusFilter = paymentStatusFilter;
            ViewBag.SortOrderDate = sortOrderDate;

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
            if (order.Payment != null)
            {
                order.Payment.Status = Models.Enums.PaymentStatusEnum.Refunded;
            }
            else
            {
                order.Payment = new Payments()
                {
                    Amount = order.TotalPrice,
                    Order = order,
                    PaymentDate = null,
                    OrderId = order.Id,
                    Status = Models.Enums.PaymentStatusEnum.Pending,
                    StripePaymentId = null
                };
                _emailService.SendEmail(new EmailData()
                {
                    EmailToId = order.Buyer.Email,
                    EmailToName = order.Buyer.UserName,
                    EmailSubject = "Payment Pending",
                    EmailBody = "The Order Payment is pending"
                });
            }
            _orderRepository.UpdateOrder(order);

            return RedirectToAction(nameof(OrderDetails), new { id });
        }
    }
}
