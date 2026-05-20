using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.Data.Interfaces;
using Servis_Centar_Za_Gitare.models;
using Servis_Centar_Za_Gitare.ViewModels;

namespace Servis_Centar_Za_Gitare.Controllers
{
    public class TechniciansController : Controller
    {
        private readonly ITechnicianRepository _technicianRepository;
        private readonly IRepairRepository _repairRepository;
        private readonly AppDbContext _context;

        public TechniciansController(ITechnicianRepository technicianRepository, IRepairRepository repairRepository, AppDbContext context)
        {
            _technicianRepository = technicianRepository;
            _repairRepository = repairRepository;
            _context = context;
        }

        public IActionResult Index()
        {
            var technicians = _technicianRepository.GetAll().OrderBy(technician => technician.Prezime).ThenBy(technician => technician.Ime);
            ViewData["Breadcrumbs"] = new[]
            {
                new BreadcrumbItemViewModel { Text = "Home", Url = Url.Action("Index", "Home") ?? "/" },
                new BreadcrumbItemViewModel { Text = "Technicians", Url = Url.Action(nameof(Index), "Technicians") ?? "/Technicians", IsActive = true }
            };
            return View(technicians);
        }

        public IActionResult Details(int id)
        {
            var technician = _technicianRepository.GetById(id);
            if (technician == null)
            {
                return NotFound();
            }

            var model = new TechnicianDetailsViewModel
            {
                Technician = technician,
                Repairs = _repairRepository.GetAll().Where(repair => repair.Tehnicar.Id == id).OrderByDescending(repair => repair.DatumOtvaranja)
            };

            ViewData["Breadcrumbs"] = new[]
            {
                new BreadcrumbItemViewModel { Text = "Home", Url = Url.Action("Index", "Home") ?? "/" },
                new BreadcrumbItemViewModel { Text = "Technicians", Url = Url.Action("Index", "Technicians") ?? "/Technicians" },
                new BreadcrumbItemViewModel { Text = technician.Ime + " " + technician.Prezime, Url = Url.Action(nameof(Details), "Technicians", new { id }) ?? "/Technicians/Details/" + id, IsActive = true }
            };

            return View(model);
        }

        [HttpGet]
        [Route("tehnicari/novi")]
        public async Task<IActionResult> Create()
        {
            var model = await BuildFormViewModelAsync(new ZapTehnicar());
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("tehnicari/novi")]
        public async Task<IActionResult> Create(TechnicianFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(await BuildFormViewModelAsync(model.Technician, model.SelectedKnowledgePairs));
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            await _context.Tehnicari.AddAsync(model.Technician);
            await _context.SaveChangesAsync();
            SyncKnowledge(model.Technician, model.SelectedKnowledgePairs);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("tehnicari/uredi/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var technician = await _context.Tehnicari
                .Include(t => t.Znanja)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (technician == null)
            {
                return NotFound();
            }

            var selectedKnowledgePairs = technician.Znanja.Select(ToKnowledgeKey).ToArray();
            return View(await BuildFormViewModelAsync(technician, selectedKnowledgePairs));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("tehnicari/uredi/{id:int}")]
        public async Task<IActionResult> Edit(int id, TechnicianFormViewModel model)
        {
            var technician = await _context.Tehnicari
                .Include(t => t.Znanja)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (technician == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.Technician.Id = id;
                return View(await BuildFormViewModelAsync(model.Technician, model.SelectedKnowledgePairs));
            }

            UpdateTechnician(technician, model.Technician);
            SyncKnowledge(technician, model.SelectedKnowledgePairs);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task<TechnicianFormViewModel> BuildFormViewModelAsync(ZapTehnicar technician, IEnumerable<string>? selectedKnowledgePairs = null)
        {
            var selectedPairs = selectedKnowledgePairs?.ToArray() ?? technician.Znanja.Select(ToKnowledgeKey).ToArray();
            return new TechnicianFormViewModel
            {
                Technician = technician,
                Poslovnice = new SelectList(await _context.Poslovnice.AsNoTracking().ToListAsync(), "Id", "Ime", technician.PoslovnicaId),
                TipGitareOptions = new SelectList(await _context.TipoveGitara.AsNoTracking().OrderBy(tip => tip.Naziv).ToListAsync(), "Id", "Naziv"),
                VrstaPopravkeOptions = new SelectList(await _context.VrstePopravke.AsNoTracking().OrderBy(vrsta => vrsta.Naziv).ToListAsync(), "Id", "Naziv"),
                KnowledgeOptions = await BuildKnowledgeOptionsAsync(selectedPairs),
                SelectedKnowledgePairs = selectedPairs
            };
        }

        private async Task<List<SelectListItem>> BuildKnowledgeOptionsAsync(IEnumerable<string> selectedKnowledgePairs)
        {
            var selected = new HashSet<string>(selectedKnowledgePairs);
            var tipoviGitara = await _context.TipoveGitara.AsNoTracking().OrderBy(tip => tip.Naziv).ToListAsync();
            var vrstePopravke = await _context.VrstePopravke.AsNoTracking().OrderBy(vrsta => vrsta.Naziv).ToListAsync();
            var options = new List<SelectListItem>();

            foreach (var tipGitare in tipoviGitara)
            {
                foreach (var vrstaPopravka in vrstePopravke)
                {
                    var key = ToKnowledgeKey(tipGitare.Id, vrstaPopravka.Id);
                    options.Add(new SelectListItem
                    {
                        Value = key,
                        Text = tipGitare.Naziv + " / " + vrstaPopravka.Naziv,
                        Selected = selected.Contains(key)
                    });
                }
            }

            return options;
        }

        private static string ToKnowledgeKey(Znanje knowledge)
        {
            return ToKnowledgeKey(knowledge.TipGitareId, knowledge.VrstaPopravkaId);
        }

        private static string ToKnowledgeKey(int tipGitareId, int vrstaPopravkaId)
        {
            return tipGitareId + ":" + vrstaPopravkaId;
        }

        private static bool TryParseKnowledgeKey(string key, out int tipGitareId, out int vrstaPopravkaId)
        {
            tipGitareId = 0;
            vrstaPopravkaId = 0;

            var parts = key.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 2
                && int.TryParse(parts[0], out tipGitareId)
                && int.TryParse(parts[1], out vrstaPopravkaId);
        }

        private static void UpdateTechnician(ZapTehnicar target, ZapTehnicar source)
        {
            target.Ime = source.Ime;
            target.Prezime = source.Prezime;
            target.Email = source.Email;
            target.BrojTelefona = source.BrojTelefona;
            target.Adresa = source.Adresa;
            target.DatumZaposlenja = source.DatumZaposlenja;
            target.Placa = source.Placa;
            target.PoslovnicaId = source.PoslovnicaId;
        }

        private void SyncKnowledge(ZapTehnicar technician, IEnumerable<string>? selectedKnowledgePairs)
        {
            var parsedKnowledge = (selectedKnowledgePairs ?? Array.Empty<string>())
                .Where(key => TryParseKnowledgeKey(key, out _, out _))
                .Select(key =>
                {
                    TryParseKnowledgeKey(key, out var tipGitareId, out var vrstaPopravkaId);
                    return new Znanje(technician.Id, tipGitareId, vrstaPopravkaId);
                })
                .ToList();

            _context.Znanja.RemoveRange(technician.Znanja.ToList());
            technician.Znanja.Clear();

            foreach (var knowledge in parsedKnowledge)
            {
                technician.Znanja.Add(knowledge);
            }
        }
    }
}
