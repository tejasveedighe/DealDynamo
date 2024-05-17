using DealDynamo.Areas.Identity.Data;
using DealDynamo.Models.UserViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DealDynamo.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> UserManager;
        private readonly RoleManager<IdentityRole> RoleManager;
        public UsersController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            RoleManager = roleManager;
            UserManager = userManager;
        }
        // GET: UsersController
        public async Task<IActionResult> Index()
        {
            string currentUserId = UserManager.GetUserId(User);
            var users = UserManager.Users.Where(x => x.Id != currentUserId).ToList();
            return View(users);
        }

        // GET: UsersController/Details/5
        public async Task<IActionResult> Details(string id)
        {
            var user = UserManager.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound("User Not Found");
            }
            UserViewModel vm = new UserViewModel()
            {
                Id = id,
                UserName = user.UserName,
                Email = user.Email,
                Role = user.IsAdmin ? "Admin" : user.IsSeller ? "Seller" : "Buyer"
            };
            return View(vm);
        }

        // GET: UsersController/Edit/5
        public ActionResult Edit(string id)
        {
            var user = UserManager.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound("User Not Found");
            }
            UserEditViewModel vm = new UserEditViewModel()
            {
                Id = id,
                UserName = user.UserName,
                Email = user.Email,
                IsAdmin = user.IsAdmin,
                IsSeller = user.IsSeller,
                IsBuyer = user.IsBuyer
            };

            return View(vm);
        }

        // POST: UsersController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserEditViewModel vm)
        {
            try
            {
                if (!vm.IsAdmin && !vm.IsSeller && !vm.IsBuyer)
                {
                    ModelState.AddModelError(string.Empty, "User Type must be selected");
                    return View("Edit", vm);
                }

                var currentUser = await UserManager.FindByEmailAsync(vm.Email);

                currentUser.Email = vm.Email;
                currentUser.UserName = vm.UserName;
                currentUser.IsAdmin = vm.IsAdmin;
                currentUser.IsBuyer = vm.IsBuyer;
                currentUser.IsSeller = vm.IsSeller;

                await UserManager.UpdateAsync(currentUser);

                string role = vm.IsAdmin ? "Admin" : vm.IsSeller ? "Seller" : "Buyer";

                await UserManager.AddToRoleAsync(currentUser, role);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: UsersController/Delete/5
        public ActionResult Delete(string id)
        {
            var user = UserManager.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound("User Not Found");
            }
            UserDeleteViewModel vm = new UserDeleteViewModel()
            {
                Id = id,
                UserName = user.UserName,
                Email = user.Email,
                IsAdmin = user.IsAdmin,
                IsSeller = user.IsSeller,
                IsBuyer = user.IsBuyer
            };

            return View(vm);
        }

        // POST: UsersController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id, IFormCollection collection)
        {
            try
            {
                var user = await UserManager.FindByIdAsync(id);
                await UserManager.DeleteAsync(user);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
