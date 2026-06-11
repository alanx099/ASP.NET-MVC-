using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.Dtos;
using Servis_Centar_Za_Gitare.models;

namespace Servis_Centar_Za_Gitare.Controllers.Api
{
    [Route("api/offices")]
    public class OfficesApiController : ApiControllerBase
    {
        public OfficesApiController(AppDbContext context) : base(context) { }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OfficeDto>>> GetAll(string? query, int take = 50)
        {
            var normalizedQuery = NormalizeQuery(query);
            var offices = Context.Poslovnice
                .AsNoTracking()
                .Include(office => office.Stranke)
                .Include(office => office.Zaposlenici)
                .Include(office => office.Nalozi)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(normalizedQuery))
            {
                offices = offices.Where(office =>
                    office.Ime.ToLower().Contains(normalizedQuery) ||
                    office.Adresa.ToLower().Contains(normalizedQuery));
            }

            var result = await offices
                .OrderBy(office => office.Ime)
                .Take(NormalizeTake(take))
                .ToListAsync();

            return result.Select(ApiDtoMapper.ToDto).ToList();
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<OfficeDto>> GetById(long id)
        {
            var office = await Context.Poslovnice
                .AsNoTracking()
                .Include(item => item.Stranke)
                .Include(item => item.Zaposlenici)
                .Include(item => item.Nalozi)
                .FirstOrDefaultAsync(item => item.Id == id);

            return office == null ? NotFound() : ApiDtoMapper.ToDto(office);
        }

        [HttpPost]
        public async Task<ActionResult<OfficeDto>> Create(OfficeRequestDto request)
        {
            var office = new Poslovnica
            {
                Ime = request.Ime.Trim(),
                Adresa = request.Adresa.Trim()
            };

            Context.Poslovnice.Add(office);
            await Context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = office.Id }, new OfficeDto(office.Id, office.Ime, office.Adresa, 0, 0, 0));
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, OfficeRequestDto request)
        {
            var office = await Context.Poslovnice.FindAsync(id);
            if (office == null)
            {
                return NotFound();
            }

            office.Ime = request.Ime.Trim();
            office.Adresa = request.Adresa.Trim();

            await Context.SaveChangesAsync();
            return NoContent();
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var office = await Context.Poslovnice.FindAsync(id);
            if (office == null)
            {
                return NotFound();
            }

            var hasRelatedRows = await Context.Stranke.AnyAsync(customer => customer.PoslovnicaId == id) ||
                                 await Context.Zaposlenici.AnyAsync(employee => employee.PoslovnicaId == id) ||
                                 await Context.Nalozi.AnyAsync(repair => repair.PoslovnicaId == id);

            if (hasRelatedRows)
            {
                return Conflict("Office cannot be deleted while customers, employees, or repairs reference it.");
            }

            Context.Poslovnice.Remove(office);
            await Context.SaveChangesAsync();
            return NoContent();
        }
    }
}
