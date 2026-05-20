using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Servis_Centar_Za_Gitare.Data.Interfaces;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.models;
using Servis_Centar_Za_Gitare.ViewModels;

namespace Servis_Centar_Za_Gitare.Controllers
{
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

        public IActionResult Index()
        {
            var customers = _customerRepository.GetAll().OrderBy(customer => customer.Prezime).ThenBy(customer => customer.Ime);
            ViewData["Breadcrumbs"] = new[]
            {
                new BreadcrumbItemViewModel { Text = "Home", Url = Url.Action("Index", "Home") ?? "/" },
                new BreadcrumbItemViewModel { Text = "Customers", Url = Url.Action(nameof(Index), "Customers") ?? "/Customers", IsActive = true }
            };
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
            TryValidateModel(model.Customer, nameof(model.Customer));

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
                        DatumZaprimanja = model.GuitarDatumZaprimanja!.Value,
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
            model.Poslovnice = new SelectList(await _context.Poslovnice.AsNoTracking().ToListAsync(), "Id", "Ime", model.Customer.PoslovnicaId);
            model.Marke = new SelectList(await _context.Marke.AsNoTracking().OrderBy(brand => brand.Naziv).ToListAsync(), "Id", "Naziv", model.GuitarMarkaId);
            model.TipoviGitare = new SelectList(await _context.TipoveGitara.AsNoTracking().OrderBy(type => type.Naziv).ToListAsync(), "Id", "Naziv", model.GuitarTipGitareId);
            return model;
        }

        // GET: customers/uredi/{id}
        [HttpGet]
        [Route("customers/uredi/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _context.Stranke.FindAsync((long)id);
            if (customer == null) return NotFound();
            ViewData["Poslovnice"] = new SelectList(_context.Poslovnice.AsNoTracking().ToList(), "Id", "Ime", customer.PoslovnicaId);
            return View(customer);
        }

        // POST: customers/uredi/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("customers/uredi/{id:int}")]
        public async Task<IActionResult> Edit(int id, Stranka stranka)
        {
            if (id != stranka.Id) return BadRequest();
            if (ModelState.IsValid)
            {
                _context.Stranke.Update(stranka);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["Poslovnice"] = new SelectList(_context.Poslovnice.AsNoTracking().ToList(), "Id", "Ime", stranka.PoslovnicaId);
            return View(stranka);
        }
    }
}