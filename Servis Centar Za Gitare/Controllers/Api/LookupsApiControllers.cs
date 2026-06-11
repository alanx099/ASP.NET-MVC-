using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.Dtos;
using Servis_Centar_Za_Gitare.models;

namespace Servis_Centar_Za_Gitare.Controllers.Api
{
    [Route("api/brands")]
    public class BrandsApiController : ApiControllerBase
    {
        public BrandsApiController(AppDbContext context) : base(context) { }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LookupDto>>> GetAll(string? query, int take = 50)
        {
            var normalizedQuery = NormalizeQuery(query);
            var items = Context.Marke.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(normalizedQuery))
            {
                items = items.Where(item => item.Naziv.ToLower().Contains(normalizedQuery));
            }

            return await items.OrderBy(item => item.Naziv).Take(NormalizeTake(take)).Select(item => new LookupDto(item.Id, item.Naziv)).ToListAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<LookupDto>> GetById(int id)
        {
            var item = await Context.Marke.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
            return item == null ? NotFound() : new LookupDto(item.Id, item.Naziv);
        }

        [HttpPost]
        public async Task<ActionResult<LookupDto>> Create(LookupRequestDto request)
        {
            var item = new Marka { Naziv = request.Naziv.Trim() };
            Context.Marke.Add(item);
            await Context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, new LookupDto(item.Id, item.Naziv));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, LookupRequestDto request)
        {
            var item = await Context.Marke.FindAsync(id);
            if (item == null) return NotFound();
            item.Naziv = request.Naziv.Trim();
            await Context.SaveChangesAsync();
            return NoContent();
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await Context.Marke.FindAsync(id);
            if (item == null) return NotFound();
            if (await Context.Gitare.AnyAsync(guitar => guitar.MarkaId == id)) return Conflict("Brand cannot be deleted while guitars reference it.");
            Context.Marke.Remove(item);
            await Context.SaveChangesAsync();
            return NoContent();
        }
    }

    [Route("api/guitar-types")]
    public class GuitarTypesApiController : ApiControllerBase
    {
        public GuitarTypesApiController(AppDbContext context) : base(context) { }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LookupDto>>> GetAll(string? query, int take = 50)
        {
            var normalizedQuery = NormalizeQuery(query);
            var items = Context.TipoveGitara.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(normalizedQuery))
            {
                items = items.Where(item => item.Naziv.ToLower().Contains(normalizedQuery));
            }

            return await items.OrderBy(item => item.Naziv).Take(NormalizeTake(take)).Select(item => new LookupDto(item.Id, item.Naziv)).ToListAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<LookupDto>> GetById(int id)
        {
            var item = await Context.TipoveGitara.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
            return item == null ? NotFound() : new LookupDto(item.Id, item.Naziv);
        }

        [HttpPost]
        public async Task<ActionResult<LookupDto>> Create(LookupRequestDto request)
        {
            var item = new TipGitare { Naziv = request.Naziv.Trim() };
            Context.TipoveGitara.Add(item);
            await Context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, new LookupDto(item.Id, item.Naziv));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, LookupRequestDto request)
        {
            var item = await Context.TipoveGitara.FindAsync(id);
            if (item == null) return NotFound();
            item.Naziv = request.Naziv.Trim();
            await Context.SaveChangesAsync();
            return NoContent();
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await Context.TipoveGitara.FindAsync(id);
            if (item == null) return NotFound();
            if (await Context.Gitare.AnyAsync(guitar => guitar.TipGitareId == id) || await Context.Znanja.AnyAsync(skill => skill.TipGitareId == id))
            {
                return Conflict("Guitar type cannot be deleted while guitars or technician knowledge reference it.");
            }

            Context.TipoveGitara.Remove(item);
            await Context.SaveChangesAsync();
            return NoContent();
        }
    }

    [Route("api/repair-statuses")]
    public class RepairStatusesApiController : ApiControllerBase
    {
        public RepairStatusesApiController(AppDbContext context) : base(context) { }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LookupDto>>> GetAll(string? query, int take = 50)
        {
            var normalizedQuery = NormalizeQuery(query);
            var items = Context.StatusiNaloga.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(normalizedQuery))
            {
                items = items.Where(item => item.Naziv.ToLower().Contains(normalizedQuery));
            }

            return await items.OrderBy(item => item.Id).Take(NormalizeTake(take)).Select(item => new LookupDto(item.Id, item.Naziv)).ToListAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<LookupDto>> GetById(int id)
        {
            var item = await Context.StatusiNaloga.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
            return item == null ? NotFound() : new LookupDto(item.Id, item.Naziv);
        }

        [HttpPost]
        public async Task<ActionResult<LookupDto>> Create(LookupRequestDto request)
        {
            var item = new StatusNaloga { Naziv = request.Naziv.Trim() };
            Context.StatusiNaloga.Add(item);
            await Context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, new LookupDto(item.Id, item.Naziv));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, LookupRequestDto request)
        {
            var item = await Context.StatusiNaloga.FindAsync(id);
            if (item == null) return NotFound();
            item.Naziv = request.Naziv.Trim();
            await Context.SaveChangesAsync();
            return NoContent();
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await Context.StatusiNaloga.FindAsync(id);
            if (item == null) return NotFound();
            if (await Context.Nalozi.AnyAsync(repair => repair.StatusNalogaId == id)) return Conflict("Repair status cannot be deleted while repair orders reference it.");
            Context.StatusiNaloga.Remove(item);
            await Context.SaveChangesAsync();
            return NoContent();
        }
    }

    [Route("api/repair-types")]
    public class RepairTypesApiController : ApiControllerBase
    {
        public RepairTypesApiController(AppDbContext context) : base(context) { }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LookupDto>>> GetAll(string? query, int take = 50)
        {
            var normalizedQuery = NormalizeQuery(query);
            var items = Context.VrstePopravke.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(normalizedQuery))
            {
                items = items.Where(item => item.Naziv.ToLower().Contains(normalizedQuery));
            }

            return await items.OrderBy(item => item.Naziv).Take(NormalizeTake(take)).Select(item => new LookupDto(item.Id, item.Naziv)).ToListAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<LookupDto>> GetById(int id)
        {
            var item = await Context.VrstePopravke.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
            return item == null ? NotFound() : new LookupDto(item.Id, item.Naziv);
        }

        [HttpPost]
        public async Task<ActionResult<LookupDto>> Create(LookupRequestDto request)
        {
            var item = new VrstaPopravka { Naziv = request.Naziv.Trim() };
            Context.VrstePopravke.Add(item);
            await Context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, new LookupDto(item.Id, item.Naziv));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, LookupRequestDto request)
        {
            var item = await Context.VrstePopravke.FindAsync(id);
            if (item == null) return NotFound();
            item.Naziv = request.Naziv.Trim();
            await Context.SaveChangesAsync();
            return NoContent();
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await Context.VrstePopravke.FindAsync(id);
            if (item == null) return NotFound();
            if (await Context.Nalozi.AnyAsync(repair => repair.VrstaPopravkaId == id) || await Context.Znanja.AnyAsync(skill => skill.VrstaPopravkaId == id))
            {
                return Conflict("Repair type cannot be deleted while repair orders or technician knowledge reference it.");
            }

            Context.VrstePopravke.Remove(item);
            await Context.SaveChangesAsync();
            return NoContent();
        }
    }
}
