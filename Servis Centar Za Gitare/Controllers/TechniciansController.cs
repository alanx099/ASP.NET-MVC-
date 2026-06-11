using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.Data.Interfaces;
using Servis_Centar_Za_Gitare.models;
using Servis_Centar_Za_Gitare.ViewModels;

namespace Servis_Centar_Za_Gitare.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
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

        public async Task<IActionResult> Index(string sort = "name", string direction = "asc", int pageSize = 10, int take = 10)
        {
            pageSize = NormalizePageSize(pageSize);
            direction = NormalizeDirection(direction);

            IQueryable<ZapTehnicar> query = _context.Tehnicari
                .AsNoTracking()
                .Include(technician => technician.Znanja)
                    .ThenInclude(skill => skill.TipGitare)
                .Include(technician => technician.Znanja)
                    .ThenInclude(skill => skill.VrstaPopravka);

            var totalCount = await query.CountAsync();
            take = NormalizeTake(take, pageSize, totalCount);

            query = sort switch
            {
                "email" => direction == "desc"
                    ? query.OrderByDescending(technician => technician.Email)
                    : query.OrderBy(technician => technician.Email),
                "skills" => direction == "desc"
                    ? query.OrderByDescending(technician => technician.Znanja.Count).ThenBy(technician => technician.Prezime)
                    : query.OrderBy(technician => technician.Znanja.Count).ThenBy(technician => technician.Prezime),
                "salary" => direction == "desc"
                    ? query.OrderByDescending(technician => technician.Placa)
                    : query.OrderBy(technician => technician.Placa),
                "newest" => direction == "desc"
                    ? query.OrderByDescending(technician => technician.Id)
                    : query.OrderBy(technician => technician.Id),
                _ => direction == "desc"
                    ? query.OrderByDescending(technician => technician.Prezime).ThenByDescending(technician => technician.Ime)
                    : query.OrderBy(technician => technician.Prezime).ThenBy(technician => technician.Ime)
            };

            var technicians = await query.Take(take).ToListAsync();
            ViewData["TechnicianRepairCounts"] = await _context.Nalozi
                .AsNoTracking()
                .Where(repair => repair.TehnicarId.HasValue)
                .GroupBy(repair => repair.TehnicarId!.Value)
                .ToDictionaryAsync(group => group.Key, group => group.Count());
            ViewData["PendingSkillNeeds"] = await BuildPendingSkillNeedsAsync();
            ViewData["Breadcrumbs"] = new[]
            {
                new BreadcrumbItemViewModel { Text = "Home", Url = Url.Action("Index", "Home") ?? "/" },
                new BreadcrumbItemViewModel { Text = "Technicians", Url = Url.Action(nameof(Index), "Technicians") ?? "/Technicians", IsActive = true }
            };
            ViewData["ListState"] = BuildListState(sort, direction, pageSize, take, totalCount, new[]
            {
                ("name", "Name"),
                ("email", "Email"),
                ("skills", "Skill count"),
                ("salary", "Salary"),
                ("newest", "Newest")
            });
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
                Repairs = _repairRepository.GetAll().Where(repair => repair.TehnicarId == id).OrderByDescending(repair => repair.DatumOtvaranja)
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
            model.Technician.PoslovnicaId = await GetDefaultOfficeIdAsync();
            ValidateTechnicianForm(model);

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

            model.Technician.Id = id;
            model.Technician.PoslovnicaId = await GetDefaultOfficeIdAsync();
            ValidateTechnicianForm(model);

            if (!ModelState.IsValid)
            {
                return View(await BuildFormViewModelAsync(model.Technician, model.SelectedKnowledgePairs));
            }

            UpdateTechnician(technician, model.Technician);
            SyncKnowledge(technician, model.SelectedKnowledgePairs);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        [Route("tehnicari/obrisi/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var technician = await _context.Tehnicari
                .Include(item => item.Znanja)
                .FirstOrDefaultAsync(item => item.Id == id);

            if (technician == null)
            {
                return NotFound();
            }

            _context.Znanja.RemoveRange(technician.Znanja);
            _context.Tehnicari.Remove(technician);
            await _context.SaveChangesAsync();
            TempData["DeleteMessage"] = "Technician deleted. Existing assigned repairs are now unassigned.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<TechnicianFormViewModel> BuildFormViewModelAsync(ZapTehnicar technician, IEnumerable<string>? selectedKnowledgePairs = null)
        {
            var selectedPairs = selectedKnowledgePairs?.ToArray() ?? technician.Znanja.Select(ToKnowledgeKey).ToArray();
            return new TechnicianFormViewModel
            {
                Technician = technician,
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

        private async Task<List<PendingSkillNeedViewModel>> BuildPendingSkillNeedsAsync()
        {
            var pending = await _context.Nalozi
                .AsNoTracking()
                .Where(repair => repair.TehnicarId == null && repair.StatusNalogaId == 1)
                .GroupBy(repair => new
                {
                    repair.Gitara.TipGitareId,
                    TipGitareName = repair.Gitara.TipGitare.Naziv,
                    repair.VrstaPopravkaId,
                    VrstaPopravkaName = repair.VrstaPopravka.Naziv
                })
                .Select(group => new PendingSkillNeedViewModel
                {
                    TipGitareId = group.Key.TipGitareId,
                    TipGitareName = group.Key.TipGitareName,
                    VrstaPopravkaId = group.Key.VrstaPopravkaId,
                    VrstaPopravkaName = group.Key.VrstaPopravkaName,
                    Count = group.Count()
                })
                .OrderBy(item => item.TipGitareName)
                .ThenBy(item => item.VrstaPopravkaName)
                .ToListAsync();

            return pending;
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

        private void ValidateTechnicianForm(TechnicianFormViewModel model)
        {
            if (!TryParseDateTime(model.Technician.DatumZaposlenja, out var hireDate))
            {
                ModelState.AddModelError(nameof(model.Technician) + "." + nameof(model.Technician.DatumZaposlenja), "Hire date must be a valid date and time.");
            }
            else if (hireDate > System.DateTime.Now.AddDays(1))
            {
                ModelState.AddModelError(nameof(model.Technician) + "." + nameof(model.Technician.DatumZaposlenja), "Hire date cannot be more than one day in the future.");
            }

            if (model.Technician.PoslovnicaId.HasValue && !_context.Poslovnice.Any(office => office.Id == model.Technician.PoslovnicaId.Value))
            {
                ModelState.AddModelError(nameof(model.Technician) + "." + nameof(model.Technician.PoslovnicaId), "Select an existing office location.");
            }

            var selectedPairs = model.SelectedKnowledgePairs ?? Array.Empty<string>();
            if (!selectedPairs.Any())
            {
                ModelState.AddModelError(nameof(model.SelectedKnowledgePairs), "Add at least one knowledge area.");
            }

            foreach (var pair in selectedPairs)
            {
                if (!TryParseKnowledgeKey(pair, out var tipGitareId, out var vrstaPopravkaId) ||
                    !_context.TipoveGitara.Any(type => type.Id == tipGitareId) ||
                    !_context.VrstePopravke.Any(type => type.Id == vrstaPopravkaId))
                {
                    ModelState.AddModelError(nameof(model.SelectedKnowledgePairs), "Knowledge areas must contain existing guitar and repair types.");
                    break;
                }
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
