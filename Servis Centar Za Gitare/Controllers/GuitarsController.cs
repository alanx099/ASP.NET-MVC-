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
        public IActionResult Index()
        {
            var guitars = _guitarRepository.GetAll().OrderBy(guitar => guitar.Marka).ThenBy(guitar => guitar.SerijskiBroj);
            ViewData["Breadcrumbs"] = new[]
            {
                new BreadcrumbItemViewModel { Text = "Home", Url = Url.Action("Index", "Home") ?? "/" },
                new BreadcrumbItemViewModel { Text = "Guitars", Url = Url.Action(nameof(Index), "Guitars") ?? "/Guitars", IsActive = true }
            };
            return View(guitars);
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

        // POST: gitare/nova
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("gitare/nova")]
        public async Task<IActionResult> Create(Gitara guitar)
        {
            if (ModelState.IsValid)
            {
                await _context.Gitare.AddAsync(guitar);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await LoadLookupsAsync();
            return View(guitar);
        }

        // GET: gitare/uredi/{id}
        [HttpGet]
        [Route("gitare/uredi/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var guitar = await _context.Gitare.FindAsync((long)id);
            if (guitar == null) return NotFound();
            await LoadLookupsAsync();
            return View(guitar);
        }

        // POST: gitare/uredi/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("gitare/uredi/{id:int}")]
        public async Task<IActionResult> Edit(int id, Gitara guitar)
        {
            if (id != guitar.Id) return BadRequest();
            if (ModelState.IsValid)
            {
                _context.Gitare.Update(guitar);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await LoadLookupsAsync();
            return View(guitar);
        }

        private async Task LoadLookupsAsync()
        {
            ViewData["Marke"] = new SelectList(await _context.Marke.AsNoTracking().ToListAsync(), "Id", "Naziv");
            ViewData["TipoviGitare"] = new SelectList(await _context.TipoveGitara.AsNoTracking().ToListAsync(), "Id", "Naziv");
            var kupci = await _context.Stranke.AsNoTracking().Select(s => new { s.Id, Text = s.Ime + " " + s.Prezime }).ToListAsync();
            ViewData["Kupci"] = new SelectList(kupci, "Id", "Text");
        }
    }
}