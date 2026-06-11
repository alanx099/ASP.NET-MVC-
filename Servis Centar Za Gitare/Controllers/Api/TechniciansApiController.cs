using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.Dtos;
using Servis_Centar_Za_Gitare.models;

namespace Servis_Centar_Za_Gitare.Controllers.Api
{
    [Route("api/technicians")]
    public class TechniciansApiController : ApiControllerBase
    {
        public TechniciansApiController(AppDbContext context) : base(context) { }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TechnicianDto>>> GetAll(string? query, long? officeId, int? guitarTypeId, int? repairTypeId, int take = 50)
        {
            var normalizedQuery = NormalizeQuery(query);
            var technicians = IncludeTechnicians(Context.Tehnicari.AsNoTracking());

            if (!string.IsNullOrWhiteSpace(normalizedQuery))
            {
                technicians = technicians.Where(technician =>
                    technician.Ime.ToLower().Contains(normalizedQuery) ||
                    technician.Prezime.ToLower().Contains(normalizedQuery) ||
                    technician.Email.ToLower().Contains(normalizedQuery));
            }

            if (officeId.HasValue)
            {
                technicians = technicians.Where(technician => technician.PoslovnicaId == officeId.Value);
            }

            if (guitarTypeId.HasValue)
            {
                technicians = technicians.Where(technician => technician.Znanja.Any(skill => skill.TipGitareId == guitarTypeId.Value));
            }

            if (repairTypeId.HasValue)
            {
                technicians = technicians.Where(technician => technician.Znanja.Any(skill => skill.VrstaPopravkaId == repairTypeId.Value));
            }

            var result = await technicians
                .OrderBy(technician => technician.Prezime)
                .ThenBy(technician => technician.Ime)
                .Take(NormalizeTake(take))
                .ToListAsync();

            return result.Select(ApiDtoMapper.ToDto).ToList();
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<TechnicianDto>> GetById(long id)
        {
            var technician = await IncludeTechnicians(Context.Tehnicari.AsNoTracking())
                .FirstOrDefaultAsync(item => item.Id == id);

            return technician == null ? NotFound() : ApiDtoMapper.ToDto(technician);
        }

        [HttpPost]
        public async Task<ActionResult<TechnicianDto>> Create(TechnicianRequestDto request)
        {
            await ValidateTechnicianAsync(request);
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var technician = new ZapTehnicar();
            ApplyEmployee(technician, request);
            Context.Tehnicari.Add(technician);
            await Context.SaveChangesAsync();

            await SyncKnowledgeAsync(technician, request.Znanja);
            await Context.SaveChangesAsync();

            var created = await IncludeTechnicians(Context.Tehnicari.AsNoTracking()).FirstAsync(item => item.Id == technician.Id);
            return CreatedAtAction(nameof(GetById), new { id = technician.Id }, ApiDtoMapper.ToDto(created));
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, TechnicianRequestDto request)
        {
            var technician = await Context.Tehnicari
                .Include(item => item.Znanja)
                .FirstOrDefaultAsync(item => item.Id == id);

            if (technician == null)
            {
                return NotFound();
            }

            await ValidateTechnicianAsync(request);
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            ApplyEmployee(technician, request);
            await SyncKnowledgeAsync(technician, request.Znanja);
            await Context.SaveChangesAsync();
            return NoContent();
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var technician = await Context.Tehnicari
                .Include(item => item.Znanja)
                .FirstOrDefaultAsync(item => item.Id == id);

            if (technician == null)
            {
                return NotFound();
            }

            Context.Znanja.RemoveRange(technician.Znanja);
            Context.Tehnicari.Remove(technician);
            await Context.SaveChangesAsync();
            return NoContent();
        }

        private static IQueryable<ZapTehnicar> IncludeTechnicians(IQueryable<ZapTehnicar> technicians)
        {
            return technicians
                .Include(technician => technician.Poslovnica)
                .Include(technician => technician.Znanja)
                    .ThenInclude(skill => skill.TipGitare)
                .Include(technician => technician.Znanja)
                    .ThenInclude(skill => skill.VrstaPopravka);
        }

        private async Task ValidateTechnicianAsync(TechnicianRequestDto request)
        {
            ValidatePastOrNearFutureDate(request.DatumZaposlenja, nameof(request.DatumZaposlenja), "Hire date");
            await ValidateOfficeAsync(request.PoslovnicaId, nameof(request.PoslovnicaId));

            var knowledge = request.Znanja.ToList();
            if (!knowledge.Any())
            {
                ModelState.AddModelError(nameof(request.Znanja), "Add at least one knowledge area.");
            }

            foreach (var skill in knowledge)
            {
                if (!await Context.TipoveGitara.AnyAsync(type => type.Id == skill.TipGitareId) ||
                    !await Context.VrstePopravke.AnyAsync(type => type.Id == skill.VrstaPopravkaId))
                {
                    ModelState.AddModelError(nameof(request.Znanja), "Knowledge areas must contain existing guitar and repair types.");
                    break;
                }
            }
        }

        private async Task SyncKnowledgeAsync(ZapTehnicar technician, IEnumerable<KnowledgeRequestDto> knowledge)
        {
            var existing = await Context.Znanja
                .Where(skill => skill.TehnicarId == technician.Id)
                .ToListAsync();

            Context.Znanja.RemoveRange(existing);

            foreach (var skill in knowledge.DistinctBy(skill => new { skill.TipGitareId, skill.VrstaPopravkaId }))
            {
                Context.Znanja.Add(new Znanje(technician.Id, skill.TipGitareId, skill.VrstaPopravkaId));
            }
        }

        private static void ApplyEmployee(Zaposlenik employee, EmployeeRequestDto request)
        {
            employee.Ime = request.Ime.Trim();
            employee.Prezime = request.Prezime.Trim();
            employee.Email = request.Email.Trim();
            employee.BrojTelefona = request.BrojTelefona.Trim();
            employee.Adresa = request.Adresa.Trim();
            employee.DatumZaposlenja = request.DatumZaposlenja.Trim();
            employee.Placa = request.Placa;
            employee.PoslovnicaId = request.PoslovnicaId;
        }
    }
}
