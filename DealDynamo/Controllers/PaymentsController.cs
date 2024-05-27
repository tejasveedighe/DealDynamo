using DealDynamo.Models.Enums;
using DealDynamo.Models.PaymentViewModels;
using DealDynamo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DealDynamo.Controllers
{
    [Authorize(Roles = "Admin, Seller")]
    public class PaymentsController : Controller
    {
        private readonly IPaymentsRepository _paymentsRepository;
        public PaymentsController(IPaymentsRepository paymentsRepository)
        {
            _paymentsRepository = paymentsRepository;
        }

        // GET: PaymentsController
        [HttpGet]
        public IActionResult Index(int currentPage = 1, int pageSize = 10, string paymentStatusFilter = "", string sortPaymentDate = "")
        {
            var payments = _paymentsRepository.GetAllPayments();

            if (!string.IsNullOrEmpty(paymentStatusFilter))
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

        // GET: PaymentsController/Details/5
        public ActionResult Details(int id)
        {
            var payment = _paymentsRepository.GetPaymentsById(id);
            return View(payment);
        }

    }
}
