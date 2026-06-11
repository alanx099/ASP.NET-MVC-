using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.Dtos;
using Servis_Centar_Za_Gitare.models;

namespace Servis_Centar_Za_Gitare.Controllers.Api
{
    [Route("api/guitars")]
    public class GuitarsApiController : ApiControllerBase
    {
        public GuitarsApiController(AppDbContext context) : base(context) { }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GuitarDto>>> GetAll(string? query, long? customerId, int? brandId, int? typeId, int take = 50)
        {
            var normalizedQuery = NormalizeQuery(query);
            var guitars = Context.Gitare
                .AsNoTracking()
                .Include(guitar => guitar.Marka)
                .Include(guitar => guitar.TipGitare)
                .Include(guitar => guitar.Kupac)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(normalizedQuery))
            {
                guitars = guitars.Where(guitar =>
                    guitar.SerijskiBroj.ToLower().Contains(normalizedQuery) ||
                    guitar.Marka.Naziv.ToLower().Contains(normalizedQuery) ||
                    guitar.TipGitare.Naziv.ToLower().Contains(normalizedQuery) ||
                    guitar.Kupac.Ime.ToLower().Contains(normalizedQuery) ||
                    guitar.Kupac.Prezime.ToLower().Contains(normalizedQuery));
            }

            if (customerId.HasValue)
            {
                guitars = guitars.Where(guitar => guitar.KupacId == customerId.Value);
            }

            if (brandId.HasValue)
            {
                guitars = guitars.Where(guitar => guitar.MarkaId == brandId.Value);
            }

            if (typeId.HasValue)
            {
                guitars = guitars.Where(guitar => guitar.TipGitareId == typeId.Value);
            }

            var result = await guitars
                .OrderBy(guitar => guitar.Marka.Naziv)
                .ThenBy(guitar => guitar.SerijskiBroj)
                .Take(NormalizeTake(take))
                .ToListAsync();

            return result.Select(ApiDtoMapper.ToDto).ToList();
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<GuitarDto>> GetById(long id)
        {
            var guitar = await Context.Gitare
                .AsNoTracking()
                .Include(item => item.Marka)
                .Include(item => item.TipGitare)
                .Include(item => item.Kupac)
                .FirstOrDefaultAsync(item => item.Id == id);

            return guitar == null ? NotFound() : ApiDtoMapper.ToDto(guitar);
        }

        [HttpPost]
        public async Task<ActionResult<GuitarDto>> Create(GuitarRequestDto request)
        {
            await ValidateGuitarAsync(request);
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var guitar = new Gitara
            {
                SerijskiBroj = request.SerijskiBroj.Trim(),
                MarkaId = request.MarkaId,
                BrojZica = request.BrojZica.Trim(),
                TipGitareId = request.TipGitareId,
                DatumZaprimanja = ToUtc(request.DatumZaprimanja),
                KupacId = request.KupacId
            };

            Context.Gitare.Add(guitar);
            await Context.SaveChangesAsync();

            var created = await FindGuitarAsync(guitar.Id);
            return CreatedAtAction(nameof(GetById), new { id = guitar.Id }, ApiDtoMapper.ToDto(created!));
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, GuitarRequestDto request)
        {
            var guitar = await Context.Gitare.FindAsync(id);
            if (guitar == null)
            {
                return NotFound();
            }

            await ValidateGuitarAsync(request);
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            guitar.SerijskiBroj = request.SerijskiBroj.Trim();
            guitar.MarkaId = request.MarkaId;
            guitar.BrojZica = request.BrojZica.Trim();
            guitar.TipGitareId = request.TipGitareId;
            guitar.DatumZaprimanja = ToUtc(request.DatumZaprimanja);
            guitar.KupacId = request.KupacId;

            await Context.SaveChangesAsync();
            return NoContent();
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var guitar = await Context.Gitare.FindAsync(id);
            if (guitar == null)
            {
                return NotFound();
            }

            if (await Context.Nalozi.AnyAsync(repair => repair.GitaraId == id))
            {
                return Conflict("Guitar cannot be deleted while it has repair orders.");
            }

            Context.Gitare.Remove(guitar);
            await Context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<Gitara?> FindGuitarAsync(long id)
        {
            return await Context.Gitare
                .AsNoTracking()
                .Include(item => item.Marka)
                .Include(item => item.TipGitare)
                .Include(item => item.Kupac)
                .FirstOrDefaultAsync(item => item.Id == id);
        }

        private async Task ValidateGuitarAsync(GuitarRequestDto request)
        {
            if (request.DatumZaprimanja == default)
            {
                ModelState.AddModelError(nameof(request.DatumZaprimanja), "Received date is required.");
            }
            else if (request.DatumZaprimanja > DateTime.Now.AddDays(1))
            {
                ModelState.AddModelError(nameof(request.DatumZaprimanja), "Received date cannot be more than one day in the future.");
            }

            if (!await Context.Marke.AnyAsync(brand => brand.Id == request.MarkaId))
            {
                ModelState.AddModelError(nameof(request.MarkaId), "Select an existing brand.");
            }

            if (!await Context.TipoveGitara.AnyAsync(type => type.Id == request.TipGitareId))
            {
                ModelState.AddModelError(nameof(request.TipGitareId), "Select an existing guitar type.");
            }

            if (!await Context.Stranke.AnyAsync(customer => customer.Id == request.KupacId))
            {
                ModelState.AddModelError(nameof(request.KupacId), "Select an existing owner.");
            }
        }
    }
}
