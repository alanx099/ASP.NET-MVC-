using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.Dtos;
using Servis_Centar_Za_Gitare.models;

namespace Servis_Centar_Za_Gitare.Controllers.Api
{
    [Route("api/customers")]
    public class CustomersApiController : ApiControllerBase
    {
        public CustomersApiController(AppDbContext context) : base(context) { }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll(string? query, long? officeId, int take = 50)
        {
            var normalizedQuery = NormalizeQuery(query);
            var customers = Context.Stranke
                .AsNoTracking()
                .Include(customer => customer.Poslovnica)
                .Include(customer => customer.Gitare)
                    .ThenInclude(guitar => guitar.Marka)
                .Include(customer => customer.Gitare)
                    .ThenInclude(guitar => guitar.TipGitare)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(normalizedQuery))
            {
                customers = customers.Where(customer =>
                    customer.Ime.ToLower().Contains(normalizedQuery) ||
                    customer.Prezime.ToLower().Contains(normalizedQuery) ||
                    customer.Email.ToLower().Contains(normalizedQuery) ||
                    customer.BrojTelefona.ToLower().Contains(normalizedQuery));
            }

            if (officeId.HasValue)
            {
                customers = customers.Where(customer => customer.PoslovnicaId == officeId.Value);
            }

            var result = await customers
                .OrderBy(customer => customer.Prezime)
                .ThenBy(customer => customer.Ime)
                .Take(NormalizeTake(take))
                .ToListAsync();

            return result.Select(ApiDtoMapper.ToDto).ToList();
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<CustomerDto>> GetById(long id)
        {
            var customer = await Context.Stranke
                .AsNoTracking()
                .Include(item => item.Poslovnica)
                .Include(item => item.Gitare)
                    .ThenInclude(guitar => guitar.Marka)
                .Include(item => item.Gitare)
                    .ThenInclude(guitar => guitar.TipGitare)
                .FirstOrDefaultAsync(item => item.Id == id);

            return customer == null ? NotFound() : ApiDtoMapper.ToDto(customer);
        }

        [HttpPost]
        public async Task<ActionResult<CustomerDto>> Create(CustomerRequestDto request)
        {
            await ValidateCustomerAsync(request);
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var customer = new Stranka
            {
                Ime = request.Ime.Trim(),
                Prezime = request.Prezime.Trim(),
                Email = request.Email.Trim(),
                BrojTelefona = request.BrojTelefona.Trim(),
                Adresa = request.Adresa.Trim(),
                DatumRegistracije = request.DatumRegistracije.Trim(),
                Napomena = request.Napomena.Trim(),
                PoslovnicaId = request.PoslovnicaId
            };

            Context.Stranke.Add(customer);
            await Context.SaveChangesAsync();

            var created = await Context.Stranke
                .AsNoTracking()
                .Include(item => item.Poslovnica)
                .Include(item => item.Gitare)
                .FirstAsync(item => item.Id == customer.Id);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiDtoMapper.ToDto(created));
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, CustomerRequestDto request)
        {
            var customer = await Context.Stranke.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            await ValidateCustomerAsync(request);
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            customer.Ime = request.Ime.Trim();
            customer.Prezime = request.Prezime.Trim();
            customer.Email = request.Email.Trim();
            customer.BrojTelefona = request.BrojTelefona.Trim();
            customer.Adresa = request.Adresa.Trim();
            customer.DatumRegistracije = request.DatumRegistracije.Trim();
            customer.Napomena = request.Napomena.Trim();
            customer.PoslovnicaId = request.PoslovnicaId;

            await Context.SaveChangesAsync();
            return NoContent();
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var customer = await Context.Stranke.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            var hasGuitars = await Context.Gitare.AnyAsync(guitar => guitar.KupacId == id);
            var hasRepairs = await Context.Nalozi.AnyAsync(repair => repair.StrankaId == id);
            if (hasGuitars || hasRepairs)
            {
                return Conflict("Customer cannot be deleted while they have guitars or repair orders.");
            }

            Context.Stranke.Remove(customer);
            await Context.SaveChangesAsync();
            return NoContent();
        }

        private async Task ValidateCustomerAsync(CustomerRequestDto request)
        {
            ValidatePastOrNearFutureDate(request.DatumRegistracije, nameof(request.DatumRegistracije), "Registration date");
            await ValidateOfficeAsync(request.PoslovnicaId, nameof(request.PoslovnicaId));
        }
    }
}
