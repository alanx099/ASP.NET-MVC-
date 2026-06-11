using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.Dtos;
using Servis_Centar_Za_Gitare.models;

namespace Servis_Centar_Za_Gitare.Controllers.Api
{
    [Route("api/employees")]
    public class EmployeesApiController : ApiControllerBase
    {
        public EmployeesApiController(AppDbContext context) : base(context) { }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll(string? query, long? officeId, int take = 50)
        {
            var normalizedQuery = NormalizeQuery(query);
            var employees = Context.Zaposlenici
                .AsNoTracking()
                .Include(employee => employee.Poslovnica)
                .Where(employee => !(employee is ZapTehnicar))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(normalizedQuery))
            {
                employees = employees.Where(employee =>
                    employee.Ime.ToLower().Contains(normalizedQuery) ||
                    employee.Prezime.ToLower().Contains(normalizedQuery) ||
                    employee.Email.ToLower().Contains(normalizedQuery));
            }

            if (officeId.HasValue)
            {
                employees = employees.Where(employee => employee.PoslovnicaId == officeId.Value);
            }

            var result = await employees
                .OrderBy(employee => employee.Prezime)
                .ThenBy(employee => employee.Ime)
                .Take(NormalizeTake(take))
                .ToListAsync();

            return result.Select(ApiDtoMapper.ToDto).ToList();
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<EmployeeDto>> GetById(long id)
        {
            var employee = await Context.Zaposlenici
                .AsNoTracking()
                .Include(item => item.Poslovnica)
                .Where(item => !(item is ZapTehnicar))
                .FirstOrDefaultAsync(item => item.Id == id);

            return employee == null ? NotFound() : ApiDtoMapper.ToDto(employee);
        }

        [HttpPost]
        public async Task<ActionResult<EmployeeDto>> Create(EmployeeRequestDto request)
        {
            await ValidateEmployeeAsync(request);
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var employee = new Zaposlenik();
            Apply(employee, request);
            Context.Zaposlenici.Add(employee);
            await Context.SaveChangesAsync();

            var created = await Context.Zaposlenici.AsNoTracking().Include(item => item.Poslovnica).FirstAsync(item => item.Id == employee.Id);
            return CreatedAtAction(nameof(GetById), new { id = employee.Id }, ApiDtoMapper.ToDto(created));
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, EmployeeRequestDto request)
        {
            var employee = await Context.Zaposlenici.FirstOrDefaultAsync(item => item.Id == id && !(item is ZapTehnicar));
            if (employee == null)
            {
                return NotFound();
            }

            await ValidateEmployeeAsync(request);
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            Apply(employee, request);
            await Context.SaveChangesAsync();
            return NoContent();
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var employee = await Context.Zaposlenici.FirstOrDefaultAsync(item => item.Id == id && !(item is ZapTehnicar));
            if (employee == null)
            {
                return NotFound();
            }

            Context.Zaposlenici.Remove(employee);
            await Context.SaveChangesAsync();
            return NoContent();
        }

        protected async Task ValidateEmployeeAsync(EmployeeRequestDto request)
        {
            ValidatePastOrNearFutureDate(request.DatumZaposlenja, nameof(request.DatumZaposlenja), "Hire date");
            await ValidateOfficeAsync(request.PoslovnicaId, nameof(request.PoslovnicaId));
        }

        protected static void Apply(Zaposlenik employee, EmployeeRequestDto request)
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
