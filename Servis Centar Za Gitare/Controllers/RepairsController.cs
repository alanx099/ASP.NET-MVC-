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
    public class RepairsController : Controller
    {
        private readonly IRepairRepository _repairRepository;
        private readonly IGuitarRepository _guitarRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ITechnicianRepository _technicianRepository;
        private readonly AppDbContext _context;

        public RepairsController(
            IRepairRepository repairRepository,
            IGuitarRepository guitarRepository,
            ICustomerRepository customerRepository,
            ITechnicianRepository technicianRepository,
            AppDbContext context)
        {
            _repairRepository = repairRepository;
            _guitarRepository = guitarRepository;
            _customerRepository = customerRepository;
            _technicianRepository = technicianRepository;
            _context = context;
        }

        [Route("servisni-nalozi")]
        public IActionResult Index()
        {
            var repairs = _repairRepository.GetAll().OrderByDescending(repair => repair.DatumOtvaranja).ThenBy(repair => repair.Id);
            ViewData["Breadcrumbs"] = new[]
            {
                new BreadcrumbItemViewModel { Text = "Home", Url = Url.Action("Index", "Home") ?? "/" },
                new BreadcrumbItemViewModel { Text = "Repairs", Url = Url.Action(nameof(Index), "Repairs") ?? "/Repairs", IsActive = true }
            };
            return View(repairs);
        }

        public IActionResult Details(int id)
        {
            var repair = _repairRepository.GetById(id);
            if (repair == null)
            {
                return NotFound();
            }

            var model = new RepairDetailsViewModel
            {
                Repair = repair,
                Guitar = _guitarRepository.GetById((int)repair.Gitara.Id),
                Customer = _customerRepository.GetById((int)repair.Stranka.Id),
                Technician = _technicianRepository.GetById((int)repair.Tehnicar.Id)
            };

            ViewData["Breadcrumbs"] = new[]
            {
                new BreadcrumbItemViewModel { Text = "Home", Url = Url.Action("Index", "Home") ?? "/" },
                new BreadcrumbItemViewModel { Text = "Repairs", Url = Url.Action("Index", "Repairs") ?? "/Repairs" },
                new BreadcrumbItemViewModel { Text = "Repair #" + id, Url = Url.Action(nameof(Details), "Repairs", new { id }) ?? "/Repairs/Details/" + id, IsActive = true }
            };

            return View(model);
        }

        // GET: servisni-nalozi/novi
        [HttpGet]
        [Route("servisni-nalozi/novi")]
        public async Task<IActionResult> Create()
        {
            await LoadLookupsAsync();
            var model = new Nalog { DatumOtvaranja = System.DateTime.Now };
            return View(model);
        }

        // POST: servisni-nalozi/novi
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("servisni-nalozi/novi")]
        public async Task<IActionResult> Create(Nalog nalog)
        {
            if (ModelState.IsValid)
            {
                await _context.Nalozi.AddAsync(nalog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await LoadLookupsAsync();
            return View(nalog);
        }

        // GET: servisni-nalozi/uredi/{id}
        [HttpGet]
        [Route("servisni-nalozi/uredi/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var nalog = await _context.Nalozi.FindAsync((long)id);
            if (nalog == null) return NotFound();
            await LoadLookupsAsync();
            return View(nalog);
        }

        // POST: servisni-nalozi/uredi/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("servisni-nalozi/uredi/{id:int}")]
        public async Task<IActionResult> Edit(int id, Nalog nalog)
        {
            if (id != nalog.Id) return BadRequest();
            if (ModelState.IsValid)
            {
                _context.Nalozi.Update(nalog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await LoadLookupsAsync();
            return View(nalog);
        }

        private async Task LoadLookupsAsync()
        {
            var gitarList = await _context.Gitare.AsNoTracking().Select(g => new { g.Id, Text = g.SerijskiBroj }).ToListAsync();
            var strankaList = await _context.Stranke.AsNoTracking().Select(s => new { s.Id, Text = s.Ime + " " + s.Prezime }).ToListAsync();
            var tehnicarList = await _context.Tehnicari.AsNoTracking().Select(t => new { t.Id, Text = t.Ime + " " + t.Prezime }).ToListAsync();
            ViewData["Gitare"] = new SelectList(gitarList, "Id", "Text");
            ViewData["Stranke"] = new SelectList(strankaList, "Id", "Text");
            ViewData["Tehnicari"] = new SelectList(tehnicarList, "Id", "Text");
            ViewData["StatusiNaloga"] = new SelectList(await _context.StatusiNaloga.AsNoTracking().ToListAsync(), "Id", "Naziv");
            ViewData["VrstePopravke"] = new SelectList(await _context.VrstePopravke.AsNoTracking().ToListAsync(), "Id", "Naziv");
        }
    }
}