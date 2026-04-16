using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Servis_Centar_Za_Gitare.Data.Interfaces;
using Servis_Centar_Za_Gitare.ViewModels;

namespace Servis_Centar_Za_Gitare.Controllers
{
    public class TechniciansController : Controller
    {
        private readonly ITechnicianRepository _technicianRepository;
        private readonly IRepairRepository _repairRepository;

        public TechniciansController(ITechnicianRepository technicianRepository, IRepairRepository repairRepository)
        {
            _technicianRepository = technicianRepository;
            _repairRepository = repairRepository;
        }

        public IActionResult Index()
        {
            var technicians = _technicianRepository.GetAll().OrderBy(technician => technician.Prezime).ThenBy(technician => technician.Ime);
            ViewData["Breadcrumbs"] = new[]
            {
                new BreadcrumbItemViewModel { Text = "Home", Url = Url.Action("Index", "Home") ?? "/" },
                new BreadcrumbItemViewModel { Text = "Technicians", Url = Url.Action(nameof(Index), "Technicians") ?? "/Technicians", IsActive = true }
            };
            return View(technicians);
        }

        public IActionResult Details(int id)
        {
            var technician = _technicianRepository.GetById(id);
            if (technician == null)
            {
                return NotFound();
            }

            var model = new TechnicianDetailsViewModel
            {
                Technician = technician,
                Repairs = _repairRepository.GetAll().Where(repair => repair.Tehnicar.Id == id).OrderByDescending(repair => repair.DatumOtvaranja)
            };

            ViewData["Breadcrumbs"] = new[]
            {
                new BreadcrumbItemViewModel { Text = "Home", Url = Url.Action("Index", "Home") ?? "/" },
                new BreadcrumbItemViewModel { Text = "Technicians", Url = Url.Action("Index", "Technicians") ?? "/Technicians" },
                new BreadcrumbItemViewModel { Text = technician.Ime + " " + technician.Prezime, Url = Url.Action(nameof(Details), "Technicians", new { id }) ?? "/Technicians/Details/" + id, IsActive = true }
            };

            return View(model);
        }
    }
}