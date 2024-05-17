using DealDynamo.Services;
using Microsoft.AspNetCore.Mvc;

namespace DealDynamo.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductRepository _productRepository;
        public CartController()
        {
            
        }
        public IActionResult ViewCart()
        {
            return View();
        }

        public IActionResult AddToCart(int id)
        {
            var product = _productRepository.GetProductById(id);
            return View();
        }
    }
}
