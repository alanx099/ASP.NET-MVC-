using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.models;

namespace Servis_Centar_Za_Gitare.Controllers.Api
{
    [ApiController]
    [Authorize(Roles = "Admin,Manager")]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected const int ReceivedStatusId = 1;
        protected readonly AppDbContext Context;

        protected ApiControllerBase(AppDbContext context)
        {
            Context = context;
        }

        protected static int NormalizeTake(int take)
        {
            return Math.Clamp(take <= 0 ? 50 : take, 1, 200);
        }

        protected static string NormalizeQuery(string? query)
        {
            return (query ?? string.Empty).Trim().ToLower();
        }

        protected static DateTime ToUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime()
            };
        }

        protected static bool TryParseDateTime(string value, out DateTime dateTime)
        {
            return DateTime.TryParseExact(
                       value,
                       new[] { "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm", "dd.MM.yyyy. HH:mm", "M/d/yyyy h:mm tt", "M/d/yyyy H:mm" },
                       CultureInfo.InvariantCulture,
                       DateTimeStyles.AllowWhiteSpaces,
                       out dateTime)
                   || DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out dateTime);
        }

        protected void ValidatePastOrNearFutureDate(string value, string key, string label)
        {
            if (!TryParseDateTime(value, out var parsed))
            {
                ModelState.AddModelError(key, $"{label} must be a valid date and time.");
            }
            else if (parsed > DateTime.Now.AddDays(1))
            {
                ModelState.AddModelError(key, $"{label} cannot be more than one day in the future.");
            }
        }

        protected async Task ValidateOfficeAsync(long? officeId, string key)
        {
            if (officeId.HasValue && !await Context.Poslovnice.AnyAsync(office => office.Id == officeId.Value))
            {
                ModelState.AddModelError(key, "Select an existing office location.");
            }
        }

        protected async Task ValidateRepairAsync(Nalog repair, bool includeClosingDate)
        {
            if (repair.DatumOtvaranja == default)
            {
                ModelState.AddModelError(nameof(repair.DatumOtvaranja), "Opening date is required.");
            }

            if (includeClosingDate && repair.DatumZatvaranja == default)
            {
                ModelState.AddModelError(nameof(repair.DatumZatvaranja), "Closing date is required.");
            }

            if (repair.DatumZatvaranja != default && repair.DatumZatvaranja < repair.DatumOtvaranja)
            {
                ModelState.AddModelError(nameof(repair.DatumZatvaranja), "Closing date cannot be before opening date.");
            }

            var guitarTypeId = await Context.Gitare
                .Where(guitar => guitar.Id == repair.GitaraId)
                .Select(guitar => (int?)guitar.TipGitareId)
                .FirstOrDefaultAsync();

            if (!guitarTypeId.HasValue)
            {
                ModelState.AddModelError(nameof(repair.GitaraId), "Select an existing guitar.");
            }

            if (!await Context.Stranke.AnyAsync(customer => customer.Id == repair.StrankaId))
            {
                ModelState.AddModelError(nameof(repair.StrankaId), "Select an existing customer.");
            }

            if (repair.TehnicarId.HasValue && !await Context.Tehnicari.AnyAsync(technician => technician.Id == repair.TehnicarId.Value))
            {
                ModelState.AddModelError(nameof(repair.TehnicarId), "Select an existing technician.");
            }

            if (!await Context.StatusiNaloga.AnyAsync(status => status.Id == repair.StatusNalogaId))
            {
                ModelState.AddModelError(nameof(repair.StatusNalogaId), "Select an existing status.");
            }

            if (!await Context.VrstePopravke.AnyAsync(type => type.Id == repair.VrstaPopravkaId))
            {
                ModelState.AddModelError(nameof(repair.VrstaPopravkaId), "Select an existing repair type.");
            }

            await ValidateOfficeAsync(repair.PoslovnicaId, nameof(repair.PoslovnicaId));

            if (!guitarTypeId.HasValue || repair.VrstaPopravkaId <= 0)
            {
                return;
            }

            var hasAnyQualifiedTechnician = await Context.Znanja.AnyAsync(skill =>
                skill.TipGitareId == guitarTypeId.Value &&
                skill.VrstaPopravkaId == repair.VrstaPopravkaId);

            if (hasAnyQualifiedTechnician && !repair.TehnicarId.HasValue)
            {
                ModelState.AddModelError(nameof(repair.TehnicarId), "Select a technician who can fulfill this repair.");
            }

            if (hasAnyQualifiedTechnician && repair.TehnicarId.HasValue)
            {
                var hasRequiredSkill = await Context.Znanja.AnyAsync(skill =>
                    skill.TehnicarId == repair.TehnicarId.Value &&
                    skill.TipGitareId == guitarTypeId.Value &&
                    skill.VrstaPopravkaId == repair.VrstaPopravkaId);

                if (!hasRequiredSkill)
                {
                    ModelState.AddModelError(nameof(repair.TehnicarId), "Missing required skill.");
                }
            }

            if (!hasAnyQualifiedTechnician)
            {
                repair.TehnicarId = null;
                repair.PoslovnicaId = null;

                if (repair.StatusNalogaId != ReceivedStatusId)
                {
                    ModelState.AddModelError(nameof(repair.StatusNalogaId), "When no technician has the required skill, status must be Zaprimljen.");
                }
            }
        }
    }
}
