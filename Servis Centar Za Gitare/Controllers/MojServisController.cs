using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.models;
using Servis_Centar_Za_Gitare.ViewModels;

namespace Servis_Centar_Za_Gitare.Controllers
{
    [Authorize(Roles = "User,Admin")]
    public class MojServisController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public MojServisController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("moj-servis")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var customer = await _context.Stranke
                .AsNoTracking()
                .Include(item => item.Gitare)
                    .ThenInclude(guitar => guitar.Marka)
                .Include(item => item.Gitare)
                    .ThenInclude(guitar => guitar.TipGitare)
                .Include(item => item.Gitare)
                    .ThenInclude(guitar => guitar.Nalozi)
                        .ThenInclude(repair => repair.StatusNaloga)
                .Include(item => item.Gitare)
                    .ThenInclude(guitar => guitar.Nalozi)
                        .ThenInclude(repair => repair.VrstaPopravka)
                .FirstOrDefaultAsync(item => item.AppUserId == user.Id);

            if (customer == null)
            {
                return View(new MyServiceViewModel
                {
                    IsLinked = false,
                    Message = "Vaš korisnički račun još nije povezan sa strankom u servisu."
                });
            }

            var model = new MyServiceViewModel
            {
                IsLinked = true,
                Guitars = customer.Gitare
                    .OrderBy(guitar => guitar.SerijskiBroj)
                    .Select(guitar => new MyGuitarViewModel
                    {
                        Id = guitar.Id,
                        Brand = guitar.Marka?.Naziv ?? string.Empty,
                        Type = guitar.TipGitare?.Naziv ?? string.Empty,
                        SerialNumber = guitar.SerijskiBroj,
                        ReceivedAt = guitar.DatumZaprimanja,
                        ServiceOrders = guitar.Nalozi
                            .OrderByDescending(repair => repair.DatumOtvaranja)
                            .Select(repair => new MyServiceOrderViewModel
                            {
                                Id = repair.Id,
                                RepairType = repair.VrstaPopravka?.Naziv ?? string.Empty,
                                Status = repair.StatusNaloga?.Naziv ?? string.Empty,
                                Description = repair.OpisKvara,
                                OpenedAt = repair.DatumOtvaranja,
                                ClosedAt = repair.DatumZatvaranja == default ? null : repair.DatumZatvaranja
                            })
                    })
            };

            return View(model);
        }
    }
}
