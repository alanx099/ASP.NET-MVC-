using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.Dtos;
using Servis_Centar_Za_Gitare.models;

namespace Servis_Centar_Za_Gitare.Controllers.Api
{
    [ApiController]
    [Authorize(Roles = "User,Admin")]
    [Route("api/me")]
    public class MeApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public MeApiController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("guitars")]
        public async Task<ActionResult<IEnumerable<MyGuitarDto>>> GetGuitars()
        {
            var customer = await GetCurrentCustomerAsync();
            if (customer == null)
            {
                return NotFound();
            }

            return await _context.Gitare
                .AsNoTracking()
                .Where(guitar => guitar.KupacId == customer.Id)
                .Include(guitar => guitar.Marka)
                .Include(guitar => guitar.TipGitare)
                .OrderBy(guitar => guitar.SerijskiBroj)
                .Select(guitar => new MyGuitarDto(
                    guitar.Id,
                    guitar.Marka.Naziv,
                    guitar.TipGitare.Naziv,
                    guitar.SerijskiBroj,
                    guitar.DatumZaprimanja))
                .ToListAsync();
        }

        [HttpGet("service-orders")]
        public async Task<ActionResult<IEnumerable<MyServiceOrderDto>>> GetServiceOrders()
        {
            var customer = await GetCurrentCustomerAsync();
            if (customer == null)
            {
                return NotFound();
            }

            return await _context.Nalozi
                .AsNoTracking()
                .Where(repair => repair.StrankaId == customer.Id)
                .Include(repair => repair.Gitara)
                    .ThenInclude(guitar => guitar.Marka)
                .Include(repair => repair.StatusNaloga)
                .Include(repair => repair.VrstaPopravka)
                .OrderByDescending(repair => repair.DatumOtvaranja)
                .Select(repair => new MyServiceOrderDto(
                    repair.Id,
                    repair.GitaraId,
                    repair.Gitara.Marka.Naziv + " " + repair.Gitara.SerijskiBroj,
                    repair.VrstaPopravka.Naziv,
                    repair.StatusNaloga.Naziv,
                    repair.OpisKvara,
                    repair.DatumOtvaranja,
                    repair.DatumZatvaranja == default ? null : repair.DatumZatvaranja))
                .ToListAsync();
        }

        private async Task<Stranka?> GetCurrentCustomerAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return null;
            }

            return await _context.Stranke.AsNoTracking().FirstOrDefaultAsync(customer => customer.AppUserId == user.Id);
        }
    }
}
