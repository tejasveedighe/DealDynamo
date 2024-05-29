using DealDynamo.Areas.Identity.Data;
using DealDynamo.Models.Enums;
using DealDynamo.Models.PaymentViewModels;
using DealDynamo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace DealDynamo.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly IPaymentsRepository _paymentsRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        public PaymentsController(IPaymentsRepository paymentsRepository, IOrderRepository orderRepository, UserManager<ApplicationUser> userManager)
        {
            _paymentsRepository = paymentsRepository;
            _orderRepository = orderRepository;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin, Seller")]
        // GET: PaymentsController
        [HttpGet]
        public IActionResult Index(int currentPage = 1, int pageSize = 10, string paymentStatusFilter = "", string sortPaymentDate = "")
        {
            var payments = _paymentsRepository.GetAllPayments();

            if (!string.IsNullOrEmpty(paymentStatusFilter) && !string.Equals(paymentStatusFilter, "All"))
            {
                payments = payments.Where(p => p.Status.ToString().Equals(paymentStatusFilter, StringComparison.OrdinalIgnoreCase));
            }

            var totalPayments = payments.Count();
            var totalPages = (int)Math.Ceiling((double)totalPayments / pageSize);

            var paginatedPayments = payments.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

            var vm = new PaymentListViewModel()
            {
                Payments = paginatedPayments,
                CurrentPage = currentPage,
                TotalPages = totalPages,
                PageSize = pageSize,
            };

            ViewBag.PaymentStatusFilter = paymentStatusFilter;
            ViewBag.SortPaymentDate = sortPaymentDate;


            return View(vm);
        }

        [Authorize(Roles = "Admin, Seller")]
        // GET: PaymentsController/Details/5
        public ActionResult Details(int id)
        {
            var payment = _paymentsRepository.GetPaymentsById(id);
            return View(payment);
        }

        [Authorize(Roles = "Buyer")]
        public async Task<IActionResult> RetryPayment(int id)
        {
            var order = _orderRepository.GetOrderById(id);
            var user = await _userManager.GetUserAsync(User);

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

            foreach (var item in order.OrderItems)
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

    }
}
