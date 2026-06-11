using Microsoft.EntityFrameworkCore;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.models;

namespace Servis_Centar_Za_Gitare.Services
{
    public class CustomerAccountLinker
    {
        private readonly AppDbContext _context;

        public CustomerAccountLinker(AppDbContext context)
        {
            _context = context;
        }

        public async Task LinkOrCreateCustomerAsync(AppUser user, string? firstName = null, string? lastName = null, string? phone = null)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return;
            }

            var normalizedEmail = user.Email.Trim().ToLower();
            var existingCustomer = await _context.Stranke
                .FirstOrDefaultAsync(customer => customer.Email.ToLower() == normalizedEmail);

            if (existingCustomer != null)
            {
                if (!string.IsNullOrWhiteSpace(existingCustomer.AppUserId) && existingCustomer.AppUserId != user.Id)
                {
                    throw new InvalidOperationException("A customer with this email is already linked to another user.");
                }

                existingCustomer.AppUserId = user.Id;
                await _context.SaveChangesAsync();
                return;
            }

            var customer = new Stranka
            {
                Ime = string.IsNullOrWhiteSpace(firstName) ? "Korisnik" : firstName.Trim(),
                Prezime = string.IsNullOrWhiteSpace(lastName) ? "Servisa" : lastName.Trim(),
                Email = user.Email.Trim(),
                BrojTelefona = string.IsNullOrWhiteSpace(phone) ? "+385000000" : phone.Trim(),
                Adresa = "Nije uneseno",
                DatumRegistracije = DateTime.Now.ToString("yyyy-MM-ddTHH:mm"),
                Napomena = string.Empty,
                AppUserId = user.Id,
                PoslovnicaId = await _context.Poslovnice
                    .AsNoTracking()
                    .OrderBy(office => office.Id)
                    .Select(office => (long?)office.Id)
                    .FirstOrDefaultAsync()
            };

            _context.Stranke.Add(customer);
            await _context.SaveChangesAsync();
        }
    }
}
