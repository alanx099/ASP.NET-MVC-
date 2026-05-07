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
        public IActionResult Create()
        {
            ViewData["Poslovnice"] = new SelectList(_context.Poslovnice.AsNoTracking().ToList(), "Id", "Ime");
            var model = new Stranka();
            return View(model);
        }

        // POST: customers/nova
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("customers/nova")]
        public async Task<IActionResult> Create(Stranka stranka)
        {
            if (ModelState.IsValid)
            {
                await _context.Stranke.AddAsync(stranka);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["Poslovnice"] = new SelectList(_context.Poslovnice.AsNoTracking().ToList(), "Id", "Ime");
            return View(stranka);
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