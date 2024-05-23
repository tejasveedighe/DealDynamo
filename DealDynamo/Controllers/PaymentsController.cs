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
        public ActionResult Index(int currentPage = 1, int pageSize = 10)
        {
            var payments = _paymentsRepository.GetAllPayments();
            var totalPayments = payments.Count();
            var totalPages = (int)Math.Ceiling((double)totalPayments / 10);

            var paginatedOrders = payments.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

            var vm = new PaymentListViewModel()
            {
                Payments = paginatedOrders,
                CurrentPage = currentPage,
                TotalPages = totalPages,
                PageSize = pageSize,
            };
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
