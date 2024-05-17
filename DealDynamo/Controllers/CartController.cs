using DealDynamo.Helper;
using DealDynamo.Models;
using DealDynamo.Models.CartViewModels;
using DealDynamo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DealDynamo.Controllers
{
    [Authorize(Roles = "Admin, Seller")]
    public class CartController : Controller
    {
        private readonly IProductRepository _productRepository;

        public CartController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [AllowAnonymous]
        public IActionResult ViewCart()
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>("cart") ?? new List<CartItem>();

            var vm = new CartViewModel
            {
                CartItems = cart,
                TotalPrice = cart.Sum(x => x.Product.Price * x.Quantity)
            };

            return View(vm);
        }

        private int isExist(int id)
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>("cart") ?? new List<CartItem>();
            for (int i = 0; i < cart.Count; i++)
            {
                if (cart[i].Product.Id == id)
                {
                    return i;
                }
            }
            return -1;
        }

        [AllowAnonymous]
        public IActionResult AddToCart(int id)
        {
            var product = _productRepository.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }

            var cart = HttpContext.Session.GetJson<List<CartItem>>("cart") ?? new List<CartItem>();

            int index = isExist(id);
            if (index != -1)
            {
                var item = cart[index];
                if (item.Product.Quantity < item.Quantity)
                {
                    cart[index].Quantity++;
                }
                else
                {
                    return Problem("Quantity cannot be more than stock");
                }
            }
            else
            {
                cart.Add(new CartItem { Product = product, Quantity = 1 });
            }

            HttpContext.Session.SetJson("cart", cart);

            return RedirectToAction(nameof(ViewCart));
        }

        [AllowAnonymous]
        public IActionResult Remove(int id)
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>("cart") ?? new List<CartItem>();
            int index = isExist(id);
            if (index == -1)
            {
                return RedirectToAction(nameof(ViewCart));
            }
            else
            {
                var item = cart[index];
                if (item == null)
                {
                    return RedirectToAction(nameof(ViewCart));
                }
                cart.Remove(item);
            }

            HttpContext.Session.SetJson("cart", cart);

            return RedirectToAction(nameof(ViewCart));
        }

        [AllowAnonymous]
        public IActionResult Decrease(int id)
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>("cart") ?? new List<CartItem>();
            int index = isExist(id);
            if (index == -1)
            {
                return RedirectToAction(nameof(ViewCart));
            }
            else
            {
                var item = cart[index];
                if (cart[index].Quantity > 1)
                    cart[index].Quantity--;
                else cart.Remove(item);
            }
            HttpContext.Session.SetJson("cart", cart);
            return RedirectToAction(nameof(ViewCart));
        }
    }
}
