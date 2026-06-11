using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.Dtos;
using Servis_Centar_Za_Gitare.models;

namespace Servis_Centar_Za_Gitare.Controllers.Api
{
    [Route("api/repairs")]
    public class RepairsApiController : ApiControllerBase
    {
        public RepairsApiController(AppDbContext context) : base(context) { }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RepairDto>>> GetAll(
            string? query,
            long? customerId,
            long? guitarId,
            long? technicianId,
            int? statusId,
            int? repairTypeId,
            int take = 50)
        {
            var normalizedQuery = NormalizeQuery(query);
            var repairs = IncludeRepairs(Context.Nalozi.AsNoTracking());

            if (!string.IsNullOrWhiteSpace(normalizedQuery))
            {
                repairs = repairs.Where(repair =>
                    repair.OpisKvara.ToLower().Contains(normalizedQuery) ||
                    repair.Stranka.Ime.ToLower().Contains(normalizedQuery) ||
                    repair.Stranka.Prezime.ToLower().Contains(normalizedQuery) ||
                    repair.Gitara.SerijskiBroj.ToLower().Contains(normalizedQuery) ||
                    repair.VrstaPopravka.Naziv.ToLower().Contains(normalizedQuery));
            }

            if (customerId.HasValue)
            {
                repairs = repairs.Where(repair => repair.StrankaId == customerId.Value);
            }

            if (guitarId.HasValue)
            {
                repairs = repairs.Where(repair => repair.GitaraId == guitarId.Value);
            }

            if (technicianId.HasValue)
            {
                repairs = repairs.Where(repair => repair.TehnicarId == technicianId.Value);
            }

            if (statusId.HasValue)
            {
                repairs = repairs.Where(repair => repair.StatusNalogaId == statusId.Value);
            }

            if (repairTypeId.HasValue)
            {
                repairs = repairs.Where(repair => repair.VrstaPopravkaId == repairTypeId.Value);
            }

            var result = await repairs
                .OrderByDescending(repair => repair.DatumOtvaranja)
                .ThenBy(repair => repair.Id)
                .Take(NormalizeTake(take))
                .ToListAsync();

            return result.Select(ApiDtoMapper.ToDto).ToList();
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<RepairDto>> GetById(long id)
        {
            var repair = await IncludeRepairs(Context.Nalozi.AsNoTracking())
                .FirstOrDefaultAsync(item => item.Id == id);

            return repair == null ? NotFound() : ApiDtoMapper.ToDto(repair);
        }

        [HttpPost]
        public async Task<ActionResult<RepairDto>> Create(RepairRequestDto request)
        {
            var repair = ToRepair(request);
            await ValidateRepairAsync(repair, includeClosingDate: false);
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            repair.DatumOtvaranja = ToUtc(repair.DatumOtvaranja);
            repair.DatumZatvaranja = request.DatumZatvaranja.HasValue ? ToUtc(request.DatumZatvaranja.Value) : default;
            repair.PoslovnicaId = await ResolveOfficeIdAsync(repair.TehnicarId, repair.PoslovnicaId);

            Context.Nalozi.Add(repair);
            await Context.SaveChangesAsync();

            var created = await IncludeRepairs(Context.Nalozi.AsNoTracking()).FirstAsync(item => item.Id == repair.Id);
            return CreatedAtAction(nameof(GetById), new { id = repair.Id }, ApiDtoMapper.ToDto(created));
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, RepairRequestDto request)
        {
            var repair = await Context.Nalozi.FindAsync(id);
            if (repair == null)
            {
                return NotFound();
            }

            repair.GitaraId = request.GitaraId;
            repair.StrankaId = request.StrankaId;
            repair.TehnicarId = request.TehnicarId;
            repair.PoslovnicaId = request.PoslovnicaId;
            repair.OpisKvara = request.OpisKvara.Trim();
            repair.DatumOtvaranja = request.DatumOtvaranja;
            repair.DatumZatvaranja = request.DatumZatvaranja ?? default;
            repair.StatusNalogaId = request.StatusNalogaId;
            repair.VrstaPopravkaId = request.VrstaPopravkaId;

            await ValidateRepairAsync(repair, includeClosingDate: false);
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            repair.DatumOtvaranja = ToUtc(repair.DatumOtvaranja);
            repair.DatumZatvaranja = request.DatumZatvaranja.HasValue ? ToUtc(request.DatumZatvaranja.Value) : default;
            repair.PoslovnicaId = await ResolveOfficeIdAsync(repair.TehnicarId, repair.PoslovnicaId);

            await Context.SaveChangesAsync();
            return NoContent();
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var repair = await Context.Nalozi.FindAsync(id);
            if (repair == null)
            {
                return NotFound();
            }

            Context.Nalozi.Remove(repair);
            await Context.SaveChangesAsync();
            return NoContent();
        }

        private static IQueryable<Nalog> IncludeRepairs(IQueryable<Nalog> repairs)
        {
            return repairs
                .Include(repair => repair.Gitara)
                    .ThenInclude(guitar => guitar.Marka)
                .Include(repair => repair.Gitara)
                    .ThenInclude(guitar => guitar.TipGitare)
                .Include(repair => repair.Stranka)
                .Include(repair => repair.Tehnicar)
                .Include(repair => repair.Poslovnica)
                .Include(repair => repair.StatusNaloga)
                .Include(repair => repair.VrstaPopravka);
        }

        private static Nalog ToRepair(RepairRequestDto request)
        {
            return new Nalog
            {
                GitaraId = request.GitaraId,
                StrankaId = request.StrankaId,
                TehnicarId = request.TehnicarId,
                PoslovnicaId = request.PoslovnicaId,
                OpisKvara = request.OpisKvara.Trim(),
                DatumOtvaranja = request.DatumOtvaranja,
                DatumZatvaranja = request.DatumZatvaranja ?? default,
                StatusNalogaId = request.StatusNalogaId,
                VrstaPopravkaId = request.VrstaPopravkaId
            };
        }

        private async Task<long?> ResolveOfficeIdAsync(long? technicianId, long? requestedOfficeId)
        {
            if (!technicianId.HasValue)
            {
                return requestedOfficeId;
            }

            return await Context.Tehnicari
                .AsNoTracking()
                .Where(technician => technician.Id == technicianId.Value)
                .Select(technician => technician.PoslovnicaId)
                .FirstOrDefaultAsync() ?? requestedOfficeId;
        }
    }
}
