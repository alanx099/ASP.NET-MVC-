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
    [Authorize(Roles = "User,Admin")]
    public class MojServisController : Controller
    {
        private static readonly int[] ActiveRepairStatusIds = { 1, 2, 3 };
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public MojServisController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("moj-servis")]
        public async Task<IActionResult> Index(string sort = "brand", string direction = "asc", int pageSize = 10, int take = 10)
        {
            pageSize = NormalizePageSize(pageSize);
            direction = NormalizeDirection(direction);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var customer = await _context.Stranke
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.AppUserId == user.Id);

            if (customer == null)
            {
                return View(new MyServiceViewModel
                {
                    IsLinked = false,
                    Message = "Vaš korisnički račun još nije povezan sa strankom u servisu."
                });
            }

            IQueryable<Gitara> query = _context.Gitare
                .AsNoTracking()
                .Where(guitar => guitar.KupacId == customer.Id)
                .Include(guitar => guitar.Marka)
                .Include(guitar => guitar.TipGitare)
                .Include(guitar => guitar.Nalozi.Where(repair => ActiveRepairStatusIds.Contains(repair.StatusNalogaId)))
                    .ThenInclude(repair => repair.StatusNaloga)
                .Include(guitar => guitar.Nalozi.Where(repair => ActiveRepairStatusIds.Contains(repair.StatusNalogaId)))
                    .ThenInclude(repair => repair.VrstaPopravka);

            var totalCount = await query.CountAsync();
            take = NormalizeTake(take, pageSize, totalCount);

            query = sort switch
            {
                "serial" => direction == "desc"
                    ? query.OrderByDescending(guitar => guitar.SerijskiBroj)
                    : query.OrderBy(guitar => guitar.SerijskiBroj),
                "type" => direction == "desc"
                    ? query.OrderByDescending(guitar => guitar.TipGitare.Naziv).ThenByDescending(guitar => guitar.SerijskiBroj)
                    : query.OrderBy(guitar => guitar.TipGitare.Naziv).ThenBy(guitar => guitar.SerijskiBroj),
                "newest" => direction == "desc"
                    ? query.OrderByDescending(guitar => guitar.DatumZaprimanja)
                    : query.OrderBy(guitar => guitar.DatumZaprimanja),
                _ => direction == "desc"
                    ? query.OrderByDescending(guitar => guitar.Marka.Naziv).ThenByDescending(guitar => guitar.SerijskiBroj)
                    : query.OrderBy(guitar => guitar.Marka.Naziv).ThenBy(guitar => guitar.SerijskiBroj)
            };

            var guitars = await query.Take(take).ToListAsync();

            ViewData["ListState"] = BuildListState(sort, direction, pageSize, take, totalCount, new[]
            {
                ("brand", "Brand"),
                ("serial", "Serial number"),
                ("type", "Guitar type"),
                ("newest", "Newest received")
            });

            var model = new MyServiceViewModel
            {
                IsLinked = true,
                Guitars = guitars
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

        private static int NormalizePageSize(int pageSize)
        {
            return pageSize == -1 || new[] { 10, 50, 100 }.Contains(pageSize) ? pageSize : 10;
        }

        private static string NormalizeDirection(string direction)
        {
            return direction == "desc" ? "desc" : "asc";
        }

        private static int NormalizeTake(int take, int pageSize, int totalCount)
        {
            if (pageSize == -1)
            {
                return totalCount;
            }

            if (totalCount <= pageSize)
            {
                return totalCount;
            }

            return Math.Min(Math.Max(take <= 0 ? pageSize : take, pageSize), totalCount);
        }

        private static ListStateViewModel BuildListState(string sort, string direction, int pageSize, int take, int totalCount, IEnumerable<(string Value, string Text)> sortOptions)
        {
            return new ListStateViewModel
            {
                Sort = sort,
                Direction = direction,
                PageSize = pageSize,
                Take = take,
                TotalCount = totalCount,
                SortOptions = sortOptions.Select(option => new SelectListItem
                {
                    Value = option.Value,
                    Text = option.Text,
                    Selected = option.Value == sort
                })
            };
        }
    }
}
