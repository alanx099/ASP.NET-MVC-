using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.Dtos;
using Servis_Centar_Za_Gitare.models;

namespace Servis_Centar_Za_Gitare.Controllers.Api
{
    [Route("api/knowledge")]
    public class KnowledgeApiController : ApiControllerBase
    {
        public KnowledgeApiController(AppDbContext context) : base(context) { }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<KnowledgeDto>>> GetAll(long? technicianId, int? guitarTypeId, int? repairTypeId, int take = 50)
        {
            var knowledge = Context.Znanja
                .AsNoTracking()
                .Include(skill => skill.TipGitare)
                .Include(skill => skill.VrstaPopravka)
                .AsQueryable();

            if (technicianId.HasValue)
            {
                knowledge = knowledge.Where(skill => skill.TehnicarId == technicianId.Value);
            }

            if (guitarTypeId.HasValue)
            {
                knowledge = knowledge.Where(skill => skill.TipGitareId == guitarTypeId.Value);
            }

            if (repairTypeId.HasValue)
            {
                knowledge = knowledge.Where(skill => skill.VrstaPopravkaId == repairTypeId.Value);
            }

            var result = await knowledge
                .OrderBy(skill => skill.TehnicarId)
                .ThenBy(skill => skill.TipGitare.Naziv)
                .ThenBy(skill => skill.VrstaPopravka.Naziv)
                .Take(NormalizeTake(take))
                .ToListAsync();

            return result.Select(ApiDtoMapper.ToDto).ToList();
        }

        [HttpGet("{technicianId:long}/{guitarTypeId:int}/{repairTypeId:int}")]
        public async Task<ActionResult<KnowledgeDto>> GetById(long technicianId, int guitarTypeId, int repairTypeId)
        {
            var knowledge = await Context.Znanja
                .AsNoTracking()
                .Include(skill => skill.TipGitare)
                .Include(skill => skill.VrstaPopravka)
                .FirstOrDefaultAsync(skill =>
                    skill.TehnicarId == technicianId &&
                    skill.TipGitareId == guitarTypeId &&
                    skill.VrstaPopravkaId == repairTypeId);

            return knowledge == null ? NotFound() : ApiDtoMapper.ToDto(knowledge);
        }

        [HttpPost]
        public async Task<ActionResult<KnowledgeDto>> Create(KnowledgeRequestDto request)
        {
            await ValidateKnowledgeAsync(request);
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var exists = await Context.Znanja.AnyAsync(skill =>
                skill.TehnicarId == request.TehnicarId &&
                skill.TipGitareId == request.TipGitareId &&
                skill.VrstaPopravkaId == request.VrstaPopravkaId);

            if (exists)
            {
                return Conflict("Knowledge area already exists for this technician.");
            }

            var knowledge = new Znanje(request.TehnicarId, request.TipGitareId, request.VrstaPopravkaId);
            Context.Znanja.Add(knowledge);
            await Context.SaveChangesAsync();

            var created = await Context.Znanja
                .AsNoTracking()
                .Include(skill => skill.TipGitare)
                .Include(skill => skill.VrstaPopravka)
                .FirstAsync(skill =>
                    skill.TehnicarId == request.TehnicarId &&
                    skill.TipGitareId == request.TipGitareId &&
                    skill.VrstaPopravkaId == request.VrstaPopravkaId);

            return CreatedAtAction(
                nameof(GetById),
                new { technicianId = request.TehnicarId, guitarTypeId = request.TipGitareId, repairTypeId = request.VrstaPopravkaId },
                ApiDtoMapper.ToDto(created));
        }

        [HttpPut("{technicianId:long}/{guitarTypeId:int}/{repairTypeId:int}")]
        public async Task<IActionResult> Update(long technicianId, int guitarTypeId, int repairTypeId, KnowledgeRequestDto request)
        {
            var existing = await Context.Znanja.FindAsync(technicianId, guitarTypeId, repairTypeId);
            if (existing == null)
            {
                return NotFound();
            }

            await ValidateKnowledgeAsync(request);
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            Context.Znanja.Remove(existing);
            Context.Znanja.Add(new Znanje(request.TehnicarId, request.TipGitareId, request.VrstaPopravkaId));
            await Context.SaveChangesAsync();
            return NoContent();
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpDelete("{technicianId:long}/{guitarTypeId:int}/{repairTypeId:int}")]
        public async Task<IActionResult> Delete(long technicianId, int guitarTypeId, int repairTypeId)
        {
            var knowledge = await Context.Znanja.FindAsync(technicianId, guitarTypeId, repairTypeId);
            if (knowledge == null)
            {
                return NotFound();
            }

            Context.Znanja.Remove(knowledge);
            await Context.SaveChangesAsync();
            return NoContent();
        }

        private async Task ValidateKnowledgeAsync(KnowledgeRequestDto request)
        {
            if (!await Context.Tehnicari.AnyAsync(technician => technician.Id == request.TehnicarId))
            {
                ModelState.AddModelError(nameof(request.TehnicarId), "Select an existing technician.");
            }

            if (!await Context.TipoveGitara.AnyAsync(type => type.Id == request.TipGitareId))
            {
                ModelState.AddModelError(nameof(request.TipGitareId), "Select an existing guitar type.");
            }

            if (!await Context.VrstePopravke.AnyAsync(type => type.Id == request.VrstaPopravkaId))
            {
                ModelState.AddModelError(nameof(request.VrstaPopravkaId), "Select an existing repair type.");
            }
        }
    }
}
