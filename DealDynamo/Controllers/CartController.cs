using DealDynamo.Areas.Identity.Data;
using DealDynamo.Data;
using DealDynamo.Helper;
using DealDynamo.Models;
using DealDynamo.Models.CartViewModels;
using DealDynamo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DealDynamo.Controllers
{
    [Authorize(Roles = "Admin, Seller")]
    public class CartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICartRepository _cartRepository;
        private UserManager<ApplicationUser> _userManager;

        public CartController(IProductRepository productRepository, DealDynamoContext db, UserManager<ApplicationUser> userManager, ICartRepository cartRepository)
        {
            _productRepository = productRepository;
            _userManager = userManager;
            _cartRepository = cartRepository;
        }

        private void GetCart()
        {
            string userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }
            var items = _cartRepository.GetCartItems(userId);
            HttpContext.Session.SetJson("cart", items);
        }

        [AllowAnonymous]
        public IActionResult ViewCart()
        {
            GetCart();
            var cart = HttpContext.Session.GetJson<List<AppCartItem>>("cart") ?? new List<AppCartItem>();

            var vm = new CartViewModel
            {
                CartItems = cart,
                TotalPrice = cart.Sum(x => x.Product.Price * x.Quantity)
            };

            return View(vm);
        }

        private int isExist(int id)
        {
            var cart = HttpContext.Session.GetJson<List<AppCartItem>>("cart") ?? new List<AppCartItem>();
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

            GetCart();

            var cart = HttpContext.Session.GetJson<List<AppCartItem>>("cart") ?? new List<AppCartItem>();

            int index = isExist(id);
            if (index != -1)
            {
                var item = cart[index];
                if (item.Product.Quantity > item.Quantity)
                {
                    if (User.Identity.IsAuthenticated)
                    {
                        _cartRepository.AddCartItem(_userManager.GetUserId(User), item);
                    }
                    else
                        cart[index].Quantity++;
                }
                else
                {
                    return Problem("Quantity cannot be more than stock");
                }
            }
            else
            {
                cart.Add(new AppCartItem { Product = product, Quantity = 1 });
            }

            HttpContext.Session.SetJson("cart", cart);

            return RedirectToAction(nameof(ViewCart));
        }

        [AllowAnonymous]
        public IActionResult Remove(int id)
        {
            var cart = HttpContext.Session.GetJson<List<AppCartItem>>("cart") ?? new List<AppCartItem>();
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
                else
                {
                    if (User.Identity.IsAuthenticated)
                    {
                        _cartRepository.RemoveCartItem(_userManager.GetUserId(User), item);
                    }
                    else
                        cart.Remove(item);
                }
            }

            HttpContext.Session.SetJson("cart", cart);

            return RedirectToAction(nameof(ViewCart));
        }

        [AllowAnonymous]
        public IActionResult Decrease(int id)
        {
            var cart = HttpContext.Session.GetJson<List<AppCartItem>>("cart") ?? new List<AppCartItem>();
            int index = isExist(id);
            if (index == -1)
            {
                return RedirectToAction(nameof(ViewCart));
            }
            else
            {
                var item = cart[index];
                if (cart[index].Quantity > 1)
                {
                    if (User.Identity.IsAuthenticated)
                    {
                        _cartRepository.DecreaseQuantity(_userManager.GetUserId(User), item);
                    }
                    else
                        cart[index].Quantity--;

                }
                else
                {
                    if (User.Identity.IsAuthenticated)
                    {
                        _cartRepository.RemoveCartItem(_userManager.GetUserId(User), item);
                    }
                    else
                        cart.Remove(item);
                }
            }
            HttpContext.Session.SetJson("cart", cart);
            return RedirectToAction(nameof(ViewCart));
        }

        [AllowAnonymous]
        public IActionResult Clear()
        {
            HttpContext.Session.Clear();
            _cartRepository.ClearCart(_userManager.GetUserId(User));
            return RedirectToAction(nameof(ViewCart));
        }
    }
}
