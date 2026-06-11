using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Servis_Centar_Za_Gitare.Data.Interfaces;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.models;
using Servis_Centar_Za_Gitare.ViewModels;

namespace Servis_Centar_Za_Gitare.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class GuitarsController : Controller
    {
        private readonly IGuitarRepository _guitarRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IRepairRepository _repairRepository;
        private readonly AppDbContext _context;

        public GuitarsController(
            IGuitarRepository guitarRepository,
            ICustomerRepository customerRepository,
            IRepairRepository repairRepository,
            AppDbContext context)
        {
            _guitarRepository = guitarRepository;
            _customerRepository = customerRepository;
            _repairRepository = repairRepository;
            _context = context;
        }

        [HttpGet]
        [Route("gitare")]
        public async Task<IActionResult> Index(string sort = "brand", string direction = "asc", long? customerId = null, int pageSize = 10, int take = 10)
        {
            pageSize = NormalizePageSize(pageSize);
            direction = NormalizeDirection(direction);

            IQueryable<Gitara> query = _context.Gitare
                .AsNoTracking()
                .Include(guitar => guitar.Marka)
                .Include(guitar => guitar.TipGitare)
                .Include(guitar => guitar.Kupac);

            if (customerId.HasValue)
            {
                query = query.Where(guitar => guitar.KupacId == customerId.Value);
            }

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

            ViewData["GuitarRepairCounts"] = await _context.Nalozi
                .AsNoTracking()
                .GroupBy(repair => repair.GitaraId)
                .ToDictionaryAsync(group => group.Key, group => group.Count());
            ViewData["Breadcrumbs"] = new[]
            {
                new BreadcrumbItemViewModel { Text = "Home", Url = Url.Action("Index", "Home") ?? "/" },
                new BreadcrumbItemViewModel { Text = "Guitars", Url = Url.Action(nameof(Index), "Guitars") ?? "/Guitars", IsActive = true }
            };
            var selectedCustomerText = customerId.HasValue
                ? await _context.Stranke
                    .AsNoTracking()
                    .Where(customer => customer.Id == customerId.Value)
                    .Select(customer => customer.Ime + " " + customer.Prezime + " - " + customer.Email)
                    .FirstOrDefaultAsync() ?? string.Empty
                : string.Empty;

            ViewData["ListState"] = BuildListState(sort, direction, pageSize, take, totalCount, customerId, selectedCustomerText, Url.Action("AutocompleteCustomers", "Guitars") ?? string.Empty, new[]
            {
                ("brand", "Brand"),
                ("serial", "Serial number"),
                ("type", "Guitar type"),
                ("newest", "Newest received")
            });
            return View(guitars);
        }

        [HttpGet]
        [Route("gitare/autocomplete/customers")]
        public async Task<IActionResult> AutocompleteCustomers(string? term)
        {
            var query = _context.Stranke.AsNoTracking();
            var normalizedTerm = (term ?? string.Empty).Trim().ToLower();

            if (!string.IsNullOrWhiteSpace(normalizedTerm))
            {
                query = query.Where(customer =>
                    (customer.Ime + " " + customer.Prezime + " " + customer.Email).ToLower().Contains(normalizedTerm));
            }

            var results = await query
                .OrderBy(customer => customer.Prezime)
                .ThenBy(customer => customer.Ime)
                .Take(12)
                .Select(customer => new
                {
                    id = customer.Id,
                    text = customer.Ime + " " + customer.Prezime + " - " + customer.Email
                })
                .ToListAsync();

            return Json(results);
        }

        public IActionResult Details(int id)
        {
            var guitar = _guitarRepository.GetById(id);
            if (guitar == null)
            {
                return NotFound();
            }

            var model = new GuitarDetailsViewModel
            {
                Guitar = guitar,
                Customer = _customerRepository.GetById((int)guitar.KupacId),
                Repair = _repairRepository.GetAll().FirstOrDefault(repair => repair.Gitara.Id == id)
            };

            ViewData["Breadcrumbs"] = new[]
            {
                new BreadcrumbItemViewModel { Text = "Home", Url = Url.Action("Index", "Home") ?? "/" },
                new BreadcrumbItemViewModel { Text = "Guitars", Url = Url.Action("Index", "Guitars") ?? "/Guitars" },
                new BreadcrumbItemViewModel { Text = guitar.SerijskiBroj, Url = Url.Action(nameof(Details), "Guitars", new { id }) ?? "/Guitars/Details/" + id, IsActive = true }
            };

            return View(model);
        }

        // GET: gitare/nova
        [HttpGet]
        [Route("gitare/nova")]
        public async Task<IActionResult> Create()
        {
            await LoadLookupsAsync();
            var model = new Gitara { DatumZaprimanja = System.DateTime.Now };
            return View(model);
        }

        [HttpGet]
        [Route("gitare/autocomplete/brands")]
        public async Task<IActionResult> AutocompleteBrands(string? term)
        {
            var query = _context.Marke.AsNoTracking();
            var normalizedTerm = (term ?? string.Empty).Trim().ToLower();

            if (!string.IsNullOrWhiteSpace(normalizedTerm))
            {
                query = query.Where(brand => brand.Naziv.ToLower().Contains(normalizedTerm));
            }

            var results = await query
                .OrderBy(brand => brand.Naziv)
                .Take(12)
                .Select(brand => new
                {
                    id = brand.Id,
                    text = brand.Naziv
                })
                .ToListAsync();

            return Json(results);
        }

        // POST: gitare/nova
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("gitare/nova")]
        public async Task<IActionResult> Create(Gitara guitar)
        {
            ValidateGuitar(guitar);

            if (ModelState.IsValid)
            {
                NormalizeGuitarDates(guitar);
                await _context.Gitare.AddAsync(guitar);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await LoadLookupsAsync(guitar);
            return View(guitar);
        }

        // GET: gitare/uredi/{id}
        [HttpGet]
        [Route("gitare/uredi/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var guitar = await _context.Gitare.FindAsync((long)id);
            if (guitar == null) return NotFound();
            await LoadLookupsAsync(guitar);
            return View(guitar);
        }

        // POST: gitare/uredi/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("gitare/uredi/{id:int}")]
        public async Task<IActionResult> Edit(int id, Gitara guitar)
        {
            if (id != guitar.Id) return BadRequest();
            ValidateGuitar(guitar);

            if (ModelState.IsValid)
            {
                NormalizeGuitarDates(guitar);
                _context.Gitare.Update(guitar);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await LoadLookupsAsync(guitar);
            return View(guitar);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        [Route("gitare/obrisi/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var guitar = await _context.Gitare.FindAsync((long)id);
            if (guitar == null)
            {
                return NotFound();
            }

            if (await _context.Nalozi.AnyAsync(repair => repair.GitaraId == guitar.Id))
            {
                TempData["DeleteMessage"] = "Guitar cannot be deleted while it has repair orders.";
                return RedirectToAction(nameof(Index));
            }

            _context.Gitare.Remove(guitar);
            await _context.SaveChangesAsync();
            TempData["DeleteMessage"] = "Guitar deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadLookupsAsync(Gitara? selectedGuitar = null)
        {
            ViewData["TipoviGitare"] = new SelectList(await _context.TipoveGitara.AsNoTracking().ToListAsync(), "Id", "Naziv");
            var kupci = await _context.Stranke.AsNoTracking().Select(s => new { s.Id, Text = s.Ime + " " + s.Prezime }).ToListAsync();
            ViewData["Kupci"] = new SelectList(kupci, "Id", "Text");

            if (selectedGuitar != null && selectedGuitar.MarkaId > 0)
            {
                ViewData["SelectedBrandText"] = await _context.Marke
                    .AsNoTracking()
                    .Where(brand => brand.Id == selectedGuitar.MarkaId)
                    .Select(brand => brand.Naziv)
                    .FirstOrDefaultAsync() ?? string.Empty;
            }
        }

        private void ValidateGuitar(Gitara guitar)
        {
            if (guitar.DatumZaprimanja == default)
            {
                ModelState.AddModelError(nameof(guitar.DatumZaprimanja), "Received date is required.");
            }

            if (guitar.DatumZaprimanja > System.DateTime.Now.AddDays(1))
            {
                ModelState.AddModelError(nameof(guitar.DatumZaprimanja), "Received date cannot be more than one day in the future.");
            }

            if (!_context.Marke.Any(brand => brand.Id == guitar.MarkaId))
            {
                ModelState.AddModelError(nameof(guitar.MarkaId), "Select an existing brand.");
            }

            if (!_context.TipoveGitara.Any(type => type.Id == guitar.TipGitareId))
            {
                ModelState.AddModelError(nameof(guitar.TipGitareId), "Select an existing guitar type.");
            }

            if (!_context.Stranke.Any(customer => customer.Id == guitar.KupacId))
            {
                ModelState.AddModelError(nameof(guitar.KupacId), "Select an existing owner.");
            }
        }

        private static void NormalizeGuitarDates(Gitara guitar)
        {
            guitar.DatumZaprimanja = ToUtc(guitar.DatumZaprimanja);
        }

        private static System.DateTime ToUtc(System.DateTime value)
        {
            return value.Kind switch
            {
                System.DateTimeKind.Utc => value,
                System.DateTimeKind.Local => value.ToUniversalTime(),
                _ => System.DateTime.SpecifyKind(value, System.DateTimeKind.Local).ToUniversalTime()
            };
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

            return System.Math.Min(System.Math.Max(take <= 0 ? pageSize : take, pageSize), totalCount);
        }

        private static ListStateViewModel BuildListState(string sort, string direction, int pageSize, int take, int totalCount, long? customerId, string customerText, string customerFilterEndpoint, IEnumerable<(string Value, string Text)> sortOptions)
        {
            return new ListStateViewModel
            {
                Sort = sort,
                Direction = direction,
                PageSize = pageSize,
                Take = take,
                TotalCount = totalCount,
                CustomerId = customerId,
                CustomerText = customerText,
                CustomerFilterEndpoint = customerFilterEndpoint,
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
