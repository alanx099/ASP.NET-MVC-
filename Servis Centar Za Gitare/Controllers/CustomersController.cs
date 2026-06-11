using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    public class CustomersController : Controller
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IRepairRepository _repairRepository;
        private readonly AppDbContext _context;

        public CustomersController(ICustomerRepository customerRepository, IRepairRepository repairRepository, AppDbContext context)
        {
            _customerRepository = customerRepository;
            _repairRepository = repairRepository;
            _context = context;
        }

        public async Task<IActionResult> Index(string sort = "name", string direction = "asc", int pageSize = 10, int take = 10)
        {
            pageSize = NormalizePageSize(pageSize);
            direction = NormalizeDirection(direction);

            var query = _context.Stranke.AsNoTracking();
            var totalCount = await query.CountAsync();
            take = NormalizeTake(take, pageSize, totalCount);

            query = sort switch
            {
                "newest" => direction == "desc"
                    ? query.OrderByDescending(customer => customer.Id)
                    : query.OrderBy(customer => customer.Id),
                "email" => direction == "desc"
                    ? query.OrderByDescending(customer => customer.Email).ThenByDescending(customer => customer.Prezime)
                    : query.OrderBy(customer => customer.Email).ThenBy(customer => customer.Prezime),
                "guitars" => direction == "desc"
                    ? query.OrderByDescending(customer => customer.Gitare.Count).ThenBy(customer => customer.Prezime)
                    : query.OrderBy(customer => customer.Gitare.Count).ThenBy(customer => customer.Prezime),
                _ => direction == "desc"
                    ? query.OrderByDescending(customer => customer.Prezime).ThenByDescending(customer => customer.Ime)
                    : query.OrderBy(customer => customer.Prezime).ThenBy(customer => customer.Ime)
            };

            var customers = await query.Take(take).ToListAsync();

            ViewData["CustomerGuitarCounts"] = await _context.Gitare
                .AsNoTracking()
                .GroupBy(guitar => guitar.KupacId)
                .ToDictionaryAsync(group => group.Key, group => group.Count());
            ViewData["CustomerRepairCounts"] = await _context.Nalozi
                .AsNoTracking()
                .GroupBy(repair => repair.StrankaId)
                .ToDictionaryAsync(group => group.Key, group => group.Count());
            ViewData["Breadcrumbs"] = new[]
            {
                new BreadcrumbItemViewModel { Text = "Home", Url = Url.Action("Index", "Home") ?? "/" },
                new BreadcrumbItemViewModel { Text = "Customers", Url = Url.Action(nameof(Index), "Customers") ?? "/Customers", IsActive = true }
            };
            ViewData["ListState"] = BuildListState(sort, direction, pageSize, take, totalCount, new[]
            {
                ("name", "Name"),
                ("email", "Email"),
                ("guitars", "Guitar count"),
                ("newest", "Newest")
            });
            return View(customers);
        }

        public IActionResult Details(int id)
        {
            var customer = _customerRepository.GetById(id);
            if (customer == null)
            {
                return NotFound();
            }

            var model = new CustomerDetailsViewModel
            {
                Customer = customer,
                Guitars = customer.Gitare.OrderBy(guitar => guitar.SerijskiBroj),
                Repairs = _repairRepository.GetAll().Where(repair => repair.Stranka.Id == id).OrderByDescending(repair => repair.DatumOtvaranja)
            };

            ViewData["Breadcrumbs"] = new[]
            {
                new BreadcrumbItemViewModel { Text = "Home", Url = Url.Action("Index", "Home") ?? "/" },
                new BreadcrumbItemViewModel { Text = "Customers", Url = Url.Action("Index", "Customers") ?? "/Customers" },
                new BreadcrumbItemViewModel { Text = customer.Ime + " " + customer.Prezime, Url = Url.Action(nameof(Details), "Customers", new { id }) ?? "/Customers/Details/" + id, IsActive = true }
            };

            return View(model);
        }

        // GET: customers/nova
        [HttpGet]
        [Route("customers/nova")]
        public async Task<IActionResult> Create()
        {
            var model = await BuildCustomerCreateViewModelAsync(new CustomerCreateViewModel());
            return View(model);
        }

        // POST: customers/nova
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("customers/nova")]
        public async Task<IActionResult> Create(CustomerCreateViewModel model)
        {
            model.Customer.PoslovnicaId = await GetDefaultOfficeIdAsync();
            TryValidateModel(model.Customer, nameof(model.Customer));
            ValidateCustomer(model.Customer, nameof(model.Customer));

            if (model.AddGuitar)
            {
                if (string.IsNullOrWhiteSpace(model.GuitarSerijskiBroj))
                {
                    ModelState.AddModelError(nameof(model.GuitarSerijskiBroj), "Serial number is required when adding a guitar.");
                }

                if (!model.GuitarMarkaId.HasValue)
                {
                    ModelState.AddModelError(nameof(model.GuitarMarkaId), "Brand is required when adding a guitar.");
                }

                if (string.IsNullOrWhiteSpace(model.GuitarBrojZica))
                {
                    ModelState.AddModelError(nameof(model.GuitarBrojZica), "Number of strings is required when adding a guitar.");
                }

                if (!model.GuitarTipGitareId.HasValue)
                {
                    ModelState.AddModelError(nameof(model.GuitarTipGitareId), "Guitar type is required when adding a guitar.");
                }

                if (!model.GuitarDatumZaprimanja.HasValue)
                {
                    ModelState.AddModelError(nameof(model.GuitarDatumZaprimanja), "Received date is required when adding a guitar.");
                }

                if (model.GuitarDatumZaprimanja.HasValue && model.GuitarDatumZaprimanja.Value > System.DateTime.Now.AddDays(1))
                {
                    ModelState.AddModelError(nameof(model.GuitarDatumZaprimanja), "Received date cannot be more than one day in the future.");
                }

                if (model.GuitarMarkaId.HasValue && !_context.Marke.Any(brand => brand.Id == model.GuitarMarkaId.Value))
                {
                    ModelState.AddModelError(nameof(model.GuitarMarkaId), "Select an existing brand.");
                }

                if (model.GuitarTipGitareId.HasValue && !_context.TipoveGitara.Any(type => type.Id == model.GuitarTipGitareId.Value))
                {
                    ModelState.AddModelError(nameof(model.GuitarTipGitareId), "Select an existing guitar type.");
                }
            }

            if (ModelState.IsValid)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                await _context.Stranke.AddAsync(model.Customer);
                await _context.SaveChangesAsync();

                if (model.AddGuitar)
                {
                    var guitar = new Gitara
                    {
                        SerijskiBroj = model.GuitarSerijskiBroj!.Trim(),
                        MarkaId = model.GuitarMarkaId!.Value,
                        BrojZica = model.GuitarBrojZica!.Trim(),
                        TipGitareId = model.GuitarTipGitareId!.Value,
                        DatumZaprimanja = ToUtc(model.GuitarDatumZaprimanja!.Value),
                        KupacId = model.Customer.Id
                    };

                    await _context.Gitare.AddAsync(guitar);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(await BuildCustomerCreateViewModelAsync(model));
        }

        private async Task<CustomerCreateViewModel> BuildCustomerCreateViewModelAsync(CustomerCreateViewModel model)
        {
            model.TipoviGitare = new SelectList(await _context.TipoveGitara.AsNoTracking().OrderBy(type => type.Naziv).ToListAsync(), "Id", "Naziv", model.GuitarTipGitareId);

            if (model.GuitarMarkaId.HasValue)
            {
                ViewData["SelectedGuitarBrandText"] = await _context.Marke
                    .AsNoTracking()
                    .Where(brand => brand.Id == model.GuitarMarkaId.Value)
                    .Select(brand => brand.Naziv)
                    .FirstOrDefaultAsync() ?? string.Empty;
            }

            return model;
        }

        // GET: customers/uredi/{id}
        [HttpGet]
        [Route("customers/uredi/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _context.Stranke.FindAsync((long)id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        // POST: customers/uredi/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("customers/uredi/{id:int}")]
        public async Task<IActionResult> Edit(int id, Stranka stranka)
        {
            if (id != stranka.Id) return BadRequest();
            stranka.PoslovnicaId = await GetDefaultOfficeIdAsync();
            ValidateCustomer(stranka);

            if (ModelState.IsValid)
            {
                _context.Stranke.Update(stranka);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(stranka);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        [Route("customers/obrisi/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _context.Stranke.FindAsync((long)id);
            if (customer == null)
            {
                return NotFound();
            }

            var hasGuitars = await _context.Gitare.AnyAsync(guitar => guitar.KupacId == customer.Id);
            var hasRepairs = await _context.Nalozi.AnyAsync(repair => repair.StrankaId == customer.Id);

            if (hasGuitars || hasRepairs)
            {
                TempData["DeleteMessage"] = "Customer cannot be deleted while they have guitars or repair orders.";
                return RedirectToAction(nameof(Index));
            }

            _context.Stranke.Remove(customer);
            await _context.SaveChangesAsync();
            TempData["DeleteMessage"] = "Customer deleted.";
            return RedirectToAction(nameof(Index));
        }

        private void ValidateCustomer(Stranka customer, string prefix = "")
        {
            var dateKey = string.IsNullOrWhiteSpace(prefix)
                ? nameof(customer.DatumRegistracije)
                : prefix + "." + nameof(customer.DatumRegistracije);

            if (!TryParseDateTime(customer.DatumRegistracije, out var registrationDate))
            {
                ModelState.AddModelError(dateKey, "Registration date must be a valid date and time.");
            }
            else if (registrationDate > System.DateTime.Now.AddDays(1))
            {
                ModelState.AddModelError(dateKey, "Registration date cannot be more than one day in the future.");
            }

            if (customer.PoslovnicaId.HasValue && !_context.Poslovnice.Any(office => office.Id == customer.PoslovnicaId.Value))
            {
                var officeKey = string.IsNullOrWhiteSpace(prefix)
                    ? nameof(customer.PoslovnicaId)
                    : prefix + "." + nameof(customer.PoslovnicaId);
                ModelState.AddModelError(officeKey, "Select an existing office location.");
            }
        }

        private async Task<long?> GetDefaultOfficeIdAsync()
        {
            return await _context.Poslovnice
                .AsNoTracking()
                .OrderBy(office => office.Id)
                .Select(office => (long?)office.Id)
                .FirstOrDefaultAsync();
        }

        private static bool TryParseDateTime(string value, out DateTime dateTime)
        {
            return DateTime.TryParseExact(
                       value,
                       new[] { "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm", "dd.MM.yyyy. HH:mm", "M/d/yyyy h:mm tt", "M/d/yyyy H:mm" },
                       CultureInfo.InvariantCulture,
                       DateTimeStyles.AllowWhiteSpaces,
                       out dateTime)
                   || DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out dateTime);
        }

        private static DateTime ToUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime()
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
