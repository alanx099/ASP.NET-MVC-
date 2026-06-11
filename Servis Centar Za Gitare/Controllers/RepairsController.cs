using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
    public class RepairsController : Controller
    {
        private const int ReceivedStatusId = 1;
        private readonly IRepairRepository _repairRepository;
        private readonly IGuitarRepository _guitarRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ITechnicianRepository _technicianRepository;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/webp"
        };
        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".webp"
        };

        public RepairsController(
            IRepairRepository repairRepository,
            IGuitarRepository guitarRepository,
            ICustomerRepository customerRepository,
            ITechnicianRepository technicianRepository,
            AppDbContext context,
            IWebHostEnvironment environment)
        {
            _repairRepository = repairRepository;
            _guitarRepository = guitarRepository;
            _customerRepository = customerRepository;
            _technicianRepository = technicianRepository;
            _context = context;
            _environment = environment;
        }

        [Route("servisni-nalozi")]
        public async Task<IActionResult> Index(string sort = "priority", string direction = "desc", int pageSize = 10, int take = 10)
        {
            pageSize = NormalizePageSize(pageSize);
            direction = NormalizeDirection(direction);

            IQueryable<Nalog> query = _context.Nalozi
                .AsNoTracking()
                .Include(repair => repair.Gitara)
                    .ThenInclude(guitar => guitar.Marka)
                .Include(repair => repair.Gitara)
                    .ThenInclude(guitar => guitar.TipGitare)
                .Include(repair => repair.Stranka)
                .Include(repair => repair.Tehnicar)
                .Include(repair => repair.StatusNaloga)
                .Include(repair => repair.VrstaPopravka);

            var totalCount = await query.CountAsync();
            take = NormalizeTake(take, pageSize, totalCount);

            query = sort switch
            {
                "opened" => direction == "desc"
                    ? query.OrderByDescending(repair => repair.DatumOtvaranja)
                    : query.OrderBy(repair => repair.DatumOtvaranja),
                "customer" => direction == "desc"
                    ? query.OrderByDescending(repair => repair.Stranka.Prezime).ThenByDescending(repair => repair.Stranka.Ime)
                    : query.OrderBy(repair => repair.Stranka.Prezime).ThenBy(repair => repair.Stranka.Ime),
                "status" => direction == "desc"
                    ? query.OrderByDescending(repair => repair.StatusNaloga.Naziv).ThenByDescending(repair => repair.DatumOtvaranja)
                    : query.OrderBy(repair => repair.StatusNaloga.Naziv).ThenByDescending(repair => repair.DatumOtvaranja),
                "type" => direction == "desc"
                    ? query.OrderByDescending(repair => repair.VrstaPopravka.Naziv).ThenByDescending(repair => repair.DatumOtvaranja)
                    : query.OrderBy(repair => repair.VrstaPopravka.Naziv).ThenByDescending(repair => repair.DatumOtvaranja),
                _ => direction == "desc"
                    ? query
                        .OrderByDescending(repair =>
                            !_context.Znanja.Any(skill =>
                                skill.TipGitareId == repair.Gitara.TipGitareId &&
                                skill.VrstaPopravkaId == repair.VrstaPopravkaId) ||
                            (repair.TehnicarId != null && !_context.Znanja.Any(skill =>
                                skill.TehnicarId == repair.TehnicarId.Value &&
                                skill.TipGitareId == repair.Gitara.TipGitareId &&
                                skill.VrstaPopravkaId == repair.VrstaPopravkaId)))
                        .ThenByDescending(repair => repair.TehnicarId == null && _context.Znanja.Any(skill =>
                            skill.TipGitareId == repair.Gitara.TipGitareId &&
                            skill.VrstaPopravkaId == repair.VrstaPopravkaId))
                        .ThenByDescending(repair => repair.DatumOtvaranja)
                        .ThenBy(repair => repair.Id)
                    : query
                        .OrderBy(repair =>
                            !_context.Znanja.Any(skill =>
                                skill.TipGitareId == repair.Gitara.TipGitareId &&
                                skill.VrstaPopravkaId == repair.VrstaPopravkaId) ||
                            (repair.TehnicarId != null && !_context.Znanja.Any(skill =>
                                skill.TehnicarId == repair.TehnicarId.Value &&
                                skill.TipGitareId == repair.Gitara.TipGitareId &&
                                skill.VrstaPopravkaId == repair.VrstaPopravkaId)))
                        .ThenBy(repair => repair.TehnicarId == null && _context.Znanja.Any(skill =>
                            skill.TipGitareId == repair.Gitara.TipGitareId &&
                            skill.VrstaPopravkaId == repair.VrstaPopravkaId))
                        .ThenBy(repair => repair.DatumOtvaranja)
                        .ThenBy(repair => repair.Id)
            };

            var selectedRepairs = await query.Take(take).ToListAsync();
            var repairs = selectedRepairs
                .Select(repair =>
                {
                    var hasQualifiedTechnician = HasAnyQualifiedTechnician(repair);
                    var assignedTechnicianIsUnqualified = repair.TehnicarId.HasValue && !TechnicianHasRequiredSkill(repair);
                    return new RepairQueueItemViewModel
                    {
                        Repair = repair,
                        MissingSkillCoverage = !hasQualifiedTechnician || assignedTechnicianIsUnqualified,
                        NeedsTechnician = repair.TehnicarId == null && hasQualifiedTechnician
                    };
                })
                .ToList();

            ViewData["Breadcrumbs"] = new[]
            {
                new BreadcrumbItemViewModel { Text = "Home", Url = Url.Action("Index", "Home") ?? "/" },
                new BreadcrumbItemViewModel { Text = "Repairs", Url = Url.Action(nameof(Index), "Repairs") ?? "/Repairs", IsActive = true }
            };
            ViewData["ListState"] = BuildListState(sort, direction, pageSize, take, totalCount, new[]
            {
                ("priority", "Priority"),
                ("opened", "Opened date"),
                ("customer", "Customer"),
                ("status", "Status"),
                ("type", "Repair type")
            });
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
                Technician = repair.TehnicarId.HasValue ? _technicianRepository.GetById((int)repair.TehnicarId.Value) : null
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
            var now = System.DateTime.UtcNow;
            var model = new Nalog
            {
                DatumOtvaranja = now,
                DatumZatvaranja = now
            };
            return View(model);
        }

        [HttpGet]
        [Route("servisni-nalozi/autocomplete/customers")]
        public async Task<IActionResult> AutocompleteCustomers(string? term)
        {
            var query = _context.Stranke.AsNoTracking();
            var normalizedTerm = NormalizeSearchTerm(term);

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

        [HttpGet]
        [Route("servisni-nalozi/autocomplete/guitars")]
        public async Task<IActionResult> AutocompleteGuitars(string? term, long? customerId)
        {
            var query = _context.Gitare.AsNoTracking();
            var normalizedTerm = NormalizeSearchTerm(term);

            if (customerId.HasValue)
            {
                query = query.Where(guitar => guitar.KupacId == customerId.Value);
            }

            if (!string.IsNullOrWhiteSpace(normalizedTerm))
            {
                query = query.Where(guitar =>
                    (guitar.Marka.Naziv + " " + guitar.TipGitare.Naziv + " " + guitar.SerijskiBroj).ToLower().Contains(normalizedTerm));
            }

            var results = await query
                .OrderBy(guitar => guitar.Marka.Naziv)
                .ThenBy(guitar => guitar.TipGitare.Naziv)
                .ThenBy(guitar => guitar.SerijskiBroj)
                .Take(12)
                .Select(guitar => new
                {
                    id = guitar.Id,
                    text = guitar.Marka.Naziv + " - " + guitar.TipGitare.Naziv + " - " + guitar.SerijskiBroj,
                    ownerId = guitar.KupacId,
                    ownerText = guitar.Kupac.Ime + " " + guitar.Kupac.Prezime + " - " + guitar.Kupac.Email,
                    typeId = guitar.TipGitareId
                })
                .ToListAsync();

            return Json(results);
        }

        [HttpGet]
        [Route("servisni-nalozi/autocomplete/repair-types")]
        public async Task<IActionResult> AutocompleteRepairTypes(string? term)
        {
            var query = _context.VrstePopravke.AsNoTracking();
            var normalizedTerm = NormalizeSearchTerm(term);

            if (!string.IsNullOrWhiteSpace(normalizedTerm))
            {
                query = query.Where(repairType => repairType.Naziv.ToLower().Contains(normalizedTerm));
            }

            var results = await query
                .OrderBy(repairType => repairType.Naziv)
                .Take(12)
                .Select(repairType => new
                {
                    id = repairType.Id,
                    text = repairType.Naziv
                })
                .ToListAsync();

            return Json(results);
        }

        [HttpGet]
        [Route("servisni-nalozi/autocomplete/technicians")]
        public async Task<IActionResult> AutocompleteTechnicians(string? term, long? guitarId, int? repairTypeId)
        {
            var query = _context.Tehnicari.AsNoTracking();
            var normalizedTerm = NormalizeSearchTerm(term);

            if (!string.IsNullOrWhiteSpace(normalizedTerm))
            {
                query = query.Where(technician =>
                    (technician.Ime + " " + technician.Prezime + " " + technician.Email).ToLower().Contains(normalizedTerm));
            }

            if (guitarId.HasValue && repairTypeId.HasValue)
            {
                var guitarTypeId = await _context.Gitare
                    .Where(guitar => guitar.Id == guitarId.Value)
                    .Select(guitar => (int?)guitar.TipGitareId)
                    .FirstOrDefaultAsync();

                if (guitarTypeId.HasValue)
                {
                    query = query.Where(technician => technician.Znanja.Any(skill =>
                        skill.TipGitareId == guitarTypeId.Value &&
                        skill.VrstaPopravkaId == repairTypeId.Value));
                }
            }

            var results = await query
                .OrderBy(technician => technician.Prezime)
                .ThenBy(technician => technician.Ime)
                .Take(12)
                .Select(technician => new
                {
                    id = technician.Id,
                    text = technician.Ime + " " + technician.Prezime
                })
                .ToListAsync();

            return Json(results);
        }

        // POST: servisni-nalozi/novi
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("servisni-nalozi/novi")]
        public async Task<IActionResult> Create(Nalog nalog)
        {
            ValidateRepair(nalog, includeClosingDate: false);

            if (ModelState.IsValid)
            {
                NormalizeDates(nalog);
                await _context.Nalozi.AddAsync(nalog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Edit), new { id = nalog.Id });
            }

            await LoadLookupsAsync(nalog);
            return View(nalog);
        }

        // GET: servisni-nalozi/uredi/{id}
        [HttpGet]
        [Route("servisni-nalozi/uredi/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var nalog = await _context.Nalozi.FindAsync((long)id);
            if (nalog == null) return NotFound();
            await LoadLookupsAsync(nalog);
            return View(nalog);
        }

        // POST: servisni-nalozi/uredi/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("servisni-nalozi/uredi/{id:int}")]
        public async Task<IActionResult> Edit(int id, Nalog nalog)
        {
            if (id != nalog.Id) return BadRequest();
            ValidateRepair(nalog, includeClosingDate: true);

            if (ModelState.IsValid)
            {
                NormalizeDates(nalog);
                _context.Nalozi.Update(nalog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await LoadLookupsAsync(nalog);
            return View(nalog);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        [Route("servisni-nalozi/obrisi/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var repair = await _context.Nalozi.FindAsync((long)id);
            if (repair == null)
            {
                return NotFound();
            }

            _context.Nalozi.Remove(repair);
            await _context.SaveChangesAsync();
            DeleteRepairUploadDirectory(repair.Id);
            TempData["DeleteMessage"] = "Repair order deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("servisni-nalozi/{id:long}/datoteke")]
        public async Task<IActionResult> Files(long id)
        {
            if (!await _context.Nalozi.AnyAsync(repair => repair.Id == id))
            {
                return NotFound();
            }

            var files = await _context.NalogDatoteke
                .AsNoTracking()
                .Where(file => file.NalogId == id)
                .OrderByDescending(file => file.DatumUploada)
                .Select(file => new
                {
                    id = file.Id,
                    name = file.OriginalniNaziv,
                    size = file.VelicinaBajtova,
                    contentType = file.TipSadrzaja,
                    uploadedAt = file.DatumUploada,
                    url = Url.Content(file.RelativnaPutanja),
                    deleteUrl = Url.Action(nameof(DeleteFile), "Repairs", new { id, fileId = file.Id })
                })
                .ToListAsync();

            return Json(files);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("servisni-nalozi/{id:long}/datoteke")]
        public async Task<IActionResult> UploadFile(long id, IFormFile? file)
        {
            if (!await _context.Nalozi.AnyAsync(repair => repair.Id == id))
            {
                return NotFound();
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Select an image file." });
            }

            var extension = Path.GetExtension(file.FileName);
            if (!AllowedImageContentTypes.Contains(file.ContentType) || !AllowedImageExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Only JPG, PNG, GIF, and WebP images are allowed." });
            }

            const long maxFileSize = 10 * 1024 * 1024;
            if (file.Length > maxFileSize)
            {
                return BadRequest(new { message = "Image can be up to 10 MB." });
            }

            var uploadDirectory = GetRepairUploadDirectory(id);
            Directory.CreateDirectory(uploadDirectory);

            var storedName = Guid.NewGuid().ToString("N") + extension.ToLowerInvariant();
            var physicalPath = Path.Combine(uploadDirectory, storedName);

            await using (var stream = System.IO.File.Create(physicalPath))
            {
                await file.CopyToAsync(stream);
            }

            var metadata = new NalogDatoteka
            {
                NalogId = id,
                OriginalniNaziv = Path.GetFileName(file.FileName),
                SpremljeniNaziv = storedName,
                RelativnaPutanja = $"/uploads/repairs/{id}/{storedName}",
                TipSadrzaja = file.ContentType,
                VelicinaBajtova = file.Length,
                DatumUploada = DateTime.UtcNow
            };

            _context.NalogDatoteke.Add(metadata);
            await _context.SaveChangesAsync();

            return Json(new
            {
                id = metadata.Id,
                name = metadata.OriginalniNaziv,
                size = metadata.VelicinaBajtova,
                contentType = metadata.TipSadrzaja,
                uploadedAt = metadata.DatumUploada,
                url = Url.Content(metadata.RelativnaPutanja),
                deleteUrl = Url.Action(nameof(DeleteFile), "Repairs", new { id, fileId = metadata.Id })
            });
        }

        [HttpDelete]
        [ValidateAntiForgeryToken]
        [Route("servisni-nalozi/{id:long}/datoteke/{fileId:long}")]
        public async Task<IActionResult> DeleteFile(long id, long fileId)
        {
            var file = await _context.NalogDatoteke.FirstOrDefaultAsync(item => item.Id == fileId && item.NalogId == id);
            if (file == null)
            {
                return NotFound();
            }

            var physicalPath = GetPhysicalPath(file.RelativnaPutanja);
            _context.NalogDatoteke.Remove(file);
            await _context.SaveChangesAsync();

            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }

            return NoContent();
        }

        private string GetRepairUploadDirectory(long repairId)
        {
            return Path.Combine(_environment.WebRootPath, "uploads", "repairs", repairId.ToString());
        }

        private string GetPhysicalPath(string relativePath)
        {
            var normalizedRelativePath = relativePath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(_environment.WebRootPath, normalizedRelativePath);
        }

        private void DeleteRepairUploadDirectory(long repairId)
        {
            var uploadDirectory = GetRepairUploadDirectory(repairId);
            if (Directory.Exists(uploadDirectory))
            {
                Directory.Delete(uploadDirectory, recursive: true);
            }
        }

        private async Task LoadLookupsAsync(Nalog? selectedRepair = null)
        {
            var gitarList = await _context.Gitare
                .AsNoTracking()
                .Select(g => new
                {
                    g.Id,
                    Text = (g.Marka != null ? g.Marka.Naziv : "Unknown brand") + " - " +
                           (g.TipGitare != null ? g.TipGitare.Naziv : "Unknown type") + " - " +
                           g.SerijskiBroj
                })
                .ToListAsync();
            var strankaList = await _context.Stranke.AsNoTracking().Select(s => new { s.Id, Text = s.Ime + " " + s.Prezime }).ToListAsync();
            var tehnicarList = await _context.Tehnicari.AsNoTracking().Select(t => new { t.Id, Text = t.Ime + " " + t.Prezime }).ToListAsync();
            ViewData["Gitare"] = new SelectList(gitarList, "Id", "Text");
            ViewData["Stranke"] = new SelectList(strankaList, "Id", "Text");
            ViewData["Tehnicari"] = new SelectList(tehnicarList, "Id", "Text");
            ViewData["StatusiNaloga"] = new SelectList(await _context.StatusiNaloga.AsNoTracking().ToListAsync(), "Id", "Naziv");
            ViewData["VrstePopravke"] = new SelectList(await _context.VrstePopravke.AsNoTracking().ToListAsync(), "Id", "Naziv");
            await LoadSelectedAutocompleteLabelsAsync(selectedRepair);
            await LoadRepairSkillCheckAsync();
        }

        private async Task LoadSelectedAutocompleteLabelsAsync(Nalog? selectedRepair)
        {
            if (selectedRepair == null)
            {
                return;
            }

            if (selectedRepair.StrankaId > 0)
            {
                ViewData["SelectedCustomerText"] = await _context.Stranke
                    .AsNoTracking()
                    .Where(customer => customer.Id == selectedRepair.StrankaId)
                    .Select(customer => customer.Ime + " " + customer.Prezime + " - " + customer.Email)
                    .FirstOrDefaultAsync() ?? string.Empty;
            }

            if (selectedRepair.GitaraId > 0)
            {
                ViewData["SelectedGuitarText"] = await _context.Gitare
                    .AsNoTracking()
                    .Where(guitar => guitar.Id == selectedRepair.GitaraId)
                    .Select(guitar => guitar.Marka.Naziv + " - " + guitar.TipGitare.Naziv + " - " + guitar.SerijskiBroj)
                    .FirstOrDefaultAsync() ?? string.Empty;
            }

            if (selectedRepair.VrstaPopravkaId > 0)
            {
                ViewData["SelectedRepairTypeText"] = await _context.VrstePopravke
                    .AsNoTracking()
                    .Where(repairType => repairType.Id == selectedRepair.VrstaPopravkaId)
                    .Select(repairType => repairType.Naziv)
                    .FirstOrDefaultAsync() ?? string.Empty;
            }

            if (selectedRepair.TehnicarId.HasValue)
            {
                ViewData["SelectedTechnicianText"] = await _context.Tehnicari
                    .AsNoTracking()
                    .Where(technician => technician.Id == selectedRepair.TehnicarId.Value)
                    .Select(technician => technician.Ime + " " + technician.Prezime)
                    .FirstOrDefaultAsync() ?? string.Empty;
            }
        }

        private static string NormalizeSearchTerm(string? term)
        {
            return (term ?? string.Empty).Trim().ToLower();
        }

        private void ValidateRepair(Nalog nalog, bool includeClosingDate)
        {
            if (nalog.DatumOtvaranja == default)
            {
                ModelState.AddModelError(nameof(nalog.DatumOtvaranja), "Opening date is required.");
            }

            if (includeClosingDate && nalog.DatumZatvaranja == default)
            {
                ModelState.AddModelError(nameof(nalog.DatumZatvaranja), "Closing date is required.");
            }

            if (includeClosingDate && nalog.DatumZatvaranja != default && nalog.DatumOtvaranja != default && nalog.DatumZatvaranja < nalog.DatumOtvaranja)
            {
                ModelState.AddModelError(nameof(nalog.DatumZatvaranja), "Closing date cannot be before opening date.");
            }

            if (!_context.Gitare.Any(guitar => guitar.Id == nalog.GitaraId))
            {
                ModelState.AddModelError(nameof(nalog.GitaraId), "Select an existing guitar.");
            }

            if (!_context.Stranke.Any(customer => customer.Id == nalog.StrankaId))
            {
                ModelState.AddModelError(nameof(nalog.StrankaId), "Select an existing customer.");
            }

            if (nalog.GitaraId > 0 && nalog.StrankaId > 0)
            {
                var guitarOwnerId = _context.Gitare
                    .Where(guitar => guitar.Id == nalog.GitaraId)
                    .Select(guitar => (long?)guitar.KupacId)
                    .FirstOrDefault();

                if (guitarOwnerId.HasValue && guitarOwnerId.Value != nalog.StrankaId)
                {
                    ModelState.AddModelError(nameof(nalog.GitaraId), "Selected guitar belongs to another customer.");
                    ModelState.AddModelError(nameof(nalog.StrankaId), "Selected customer must own the selected guitar.");
                }
            }

            var hasAnyQualifiedTechnician = HasAnyQualifiedTechnician(nalog);

            if (nalog.TehnicarId.HasValue && !_context.Tehnicari.Any(technician => technician.Id == nalog.TehnicarId.Value))
            {
                ModelState.AddModelError(nameof(nalog.TehnicarId), "Select an existing technician.");
            }

            if (!_context.StatusiNaloga.Any(status => status.Id == nalog.StatusNalogaId))
            {
                ModelState.AddModelError(nameof(nalog.StatusNalogaId), "Select an existing status.");
            }

            if (!_context.VrstePopravke.Any(type => type.Id == nalog.VrstaPopravkaId))
            {
                ModelState.AddModelError(nameof(nalog.VrstaPopravkaId), "Select an existing repair type.");
            }

            if (hasAnyQualifiedTechnician && !nalog.TehnicarId.HasValue)
            {
                ModelState.AddModelError(nameof(nalog.TehnicarId), "Select a technician who can fulfill this repair.");
            }

            if (hasAnyQualifiedTechnician && !TechnicianHasRequiredSkill(nalog))
            {
                ModelState.AddModelError(nameof(nalog.TehnicarId), "Missing required skill");
                ModelState.AddModelError(nameof(nalog.VrstaPopravkaId), "Missing required skill");
            }

            if (!hasAnyQualifiedTechnician && nalog.GitaraId > 0 && nalog.VrstaPopravkaId > 0)
            {
                nalog.TehnicarId = null;

                if (nalog.StatusNalogaId != ReceivedStatusId)
                {
                    ModelState.AddModelError(nameof(nalog.StatusNalogaId), "When no technician has the required skill, status must be Zaprimljen.");
                }
            }
        }

        private bool TechnicianHasRequiredSkill(Nalog nalog)
        {
            if (nalog.GitaraId <= 0 || !nalog.TehnicarId.HasValue || nalog.VrstaPopravkaId <= 0)
            {
                return true;
            }

            var guitarTypeId = _context.Gitare
                .Where(guitar => guitar.Id == nalog.GitaraId)
                .Select(guitar => (int?)guitar.TipGitareId)
                .FirstOrDefault();

            if (!guitarTypeId.HasValue)
            {
                return true;
            }

            return _context.Znanja.Any(skill =>
                skill.TehnicarId == nalog.TehnicarId.Value &&
                skill.TipGitareId == guitarTypeId.Value &&
                skill.VrstaPopravkaId == nalog.VrstaPopravkaId);
        }

        private bool HasAnyQualifiedTechnician(Nalog nalog)
        {
            if (nalog.GitaraId <= 0 || nalog.VrstaPopravkaId <= 0)
            {
                return true;
            }

            var guitarTypeId = _context.Gitare
                .Where(guitar => guitar.Id == nalog.GitaraId)
                .Select(guitar => (int?)guitar.TipGitareId)
                .FirstOrDefault();

            return !guitarTypeId.HasValue || _context.Znanja.Any(skill =>
                skill.TipGitareId == guitarTypeId.Value &&
                skill.VrstaPopravkaId == nalog.VrstaPopravkaId);
        }

        private async Task LoadRepairSkillCheckAsync()
        {
            var guitars = await _context.Gitare
                .AsNoTracking()
                .Select(guitar => new
                {
                    id = guitar.Id,
                    ownerId = guitar.KupacId,
                    ownerText = guitar.Kupac.Ime + " " + guitar.Kupac.Prezime + " - " + guitar.Kupac.Email,
                    typeId = guitar.TipGitareId,
                    name = guitar.SerijskiBroj,
                    typeName = guitar.TipGitare.Naziv
                })
                .ToListAsync();

            var technicianSkills = await _context.Tehnicari
                .AsNoTracking()
                .Select(technician => new
                {
                    id = technician.Id,
                    name = technician.Ime + " " + technician.Prezime,
                    skills = technician.Znanja.Select(skill => new
                    {
                        typeId = skill.TipGitareId,
                        repairTypeId = skill.VrstaPopravkaId
                    })
                })
                .ToListAsync();

            var repairTypes = await _context.VrstePopravke
                .AsNoTracking()
                .Select(repairType => new
                {
                    id = repairType.Id,
                    name = repairType.Naziv
                })
                .ToListAsync();

            ViewData["RepairSkillCheck"] = JsonSerializer.Serialize(new
            {
                guitars,
                technicianSkills,
                repairTypes,
                receivedStatusId = ReceivedStatusId
            });
        }

        private static void NormalizeDates(Nalog nalog)
        {
            nalog.DatumOtvaranja = ToUtc(nalog.DatumOtvaranja);

            if (nalog.DatumZatvaranja == default)
            {
                nalog.DatumZatvaranja = nalog.DatumOtvaranja;
            }
            else
            {
                nalog.DatumZatvaranja = ToUtc(nalog.DatumZatvaranja);
            }

            if (nalog.TehnicarId == null)
            {
                nalog.PoslovnicaId = null;
            }
        }

        private static DateTime ToUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
        }

        private static int NormalizePageSize(int pageSize)
        {
            return pageSize == -1 || new[] { 10, 50, 100 }.Contains(pageSize) ? pageSize : 10;
        }

        private static string NormalizeDirection(string direction)
        {
            return direction == "asc" ? "asc" : "desc";
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
