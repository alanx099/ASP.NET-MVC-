using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.models;
using Servis_Centar_Za_Gitare.ViewModels;

namespace Servis_Centar_Za_Gitare.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementController(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .Include(user => user.Stranka)
                .OrderBy(user => user.Email)
                .ToListAsync();

            var items = new List<UserManagementUserViewModel>();
            foreach (var user in users)
            {
                items.Add(new UserManagementUserViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? user.UserName ?? string.Empty,
                    Roles = await _userManager.GetRolesAsync(user),
                    CustomerName = user.Stranka == null ? null : user.Stranka.Ime + " " + user.Stranka.Prezime,
                    LockedOut = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow
                });
            }

            return View(new UserManagementIndexViewModel { Users = items });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.Users.Include(item => item.Stranka).FirstOrDefaultAsync(item => item.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(await BuildEditModelAsync(user));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserManagementEditViewModel model)
        {
            var user = await _userManager.Users.Include(item => item.Stranka).FirstOrDefaultAsync(item => item.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, roles);
            await _userManager.AddToRolesAsync(user, model.SelectedRoles ?? Array.Empty<string>());

            var currentlyLinked = await _context.Stranke.FirstOrDefaultAsync(customer => customer.AppUserId == user.Id);
            if (currentlyLinked != null && currentlyLinked.Id != model.StrankaId)
            {
                currentlyLinked.AppUserId = null;
            }

            if (model.StrankaId.HasValue)
            {
                var customer = await _context.Stranke.FirstOrDefaultAsync(item => item.Id == model.StrankaId.Value);
                if (customer == null)
                {
                    return NotFound();
                }

                if (!string.IsNullOrWhiteSpace(customer.AppUserId) && customer.AppUserId != user.Id)
                {
                    ModelState.AddModelError(nameof(model.StrankaId), "Selected customer is already linked to another user.");
                    return View(await BuildEditModelAsync(user, model));
                }

                customer.AppUserId = user.Id;
            }

            if (model.LockedOut)
            {
                await _userManager.SetLockoutEnabledAsync(user, true);
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<UserManagementEditViewModel> BuildEditModelAsync(AppUser user, UserManagementEditViewModel? posted = null)
        {
            var selectedRoles = posted?.SelectedRoles ?? (await _userManager.GetRolesAsync(user)).ToArray();
            var selectedCustomerId = posted?.StrankaId ?? user.Stranka?.Id;
            var roles = await _roleManager.Roles.OrderBy(role => role.Name).Select(role => role.Name!).ToListAsync();
            var customers = await _context.Stranke
                .AsNoTracking()
                .Where(customer => customer.AppUserId == null || customer.AppUserId == user.Id)
                .OrderBy(customer => customer.Prezime)
                .ThenBy(customer => customer.Ime)
                .ToListAsync();

            return new UserManagementEditViewModel
            {
                Id = user.Id,
                Email = user.Email ?? user.UserName ?? string.Empty,
                SelectedRoles = selectedRoles,
                StrankaId = selectedCustomerId,
                LockedOut = posted?.LockedOut ?? (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow),
                RoleOptions = roles.Select(role => new SelectListItem(role, role, selectedRoles.Contains(role))),
                CustomerOptions = customers.Select(customer => new SelectListItem(customer.Ime + " " + customer.Prezime + " - " + customer.Email, customer.Id.ToString(), customer.Id == selectedCustomerId))
            };
        }
    }
}
