// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using DealDynamo.Areas.Identity.Data;
using DealDynamo.Data;
using DealDynamo.Helper;
using DealDynamo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DealDynamo.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly DealDynamoContext _db;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger, DealDynamoContext db, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            string userId = _userManager.GetUserId(User);

            var currentUserCart = HttpContext.Session.GetJson<List<AppCartItem>>("cart") ?? new List<AppCartItem>();


            if (userId != null)
            {
                var dbCartItems = _db.CartItems.Where(c => c.UserId == Guid.Parse(userId)).ToList();
                foreach (var currentUserItem in currentUserCart)
                {
                    var dbItem = dbCartItems.FirstOrDefault(c => c.ProductId == currentUserItem.Product.Id);
                    if (dbItem != null)
                    {
                        dbItem.Quantity = currentUserItem.Quantity;
                        _db.CartItems.Update(dbItem);
                    }
                    else
                    {
                        _db.CartItems.Add(new CartItem()
                        {
                            ProductId = currentUserItem.Product.Id,
                            Quantity = currentUserItem.Quantity,
                            UserId = Guid.Parse(userId)
                        });
                    }
                }

                // Remove items from the database that are not in the session cart
                foreach (var dbItem in dbCartItems)
                {
                    if (!currentUserCart.Any(ci => ci.Product.Id == dbItem.ProductId))
                    {
                        _db.CartItems.Remove(dbItem);
                    }
                }

            }
            _db.SaveChanges();

            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");

            // clear the session on logout
            HttpContext.Session.Clear();
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // This needs to be a redirect so that the browser performs a new
                // request and the identity for the user gets updated.
                return RedirectToPage();
            }
        }
    }
}
