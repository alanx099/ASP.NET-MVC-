using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Servis_Centar_Za_Gitare.Data.Interfaces;
using Servis_Centar_Za_Gitare.ViewModels;

namespace Servis_Centar_Za_Gitare.Controllers
{
    public class RepairsController : Controller
    {
        private readonly IRepairRepository _repairRepository;
        private readonly IGuitarRepository _guitarRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ITechnicianRepository _technicianRepository;

        public RepairsController(
            IRepairRepository repairRepository,
            IGuitarRepository guitarRepository,
            ICustomerRepository customerRepository,
            ITechnicianRepository technicianRepository)
        {
            _repairRepository = repairRepository;
            _guitarRepository = guitarRepository;
            _customerRepository = customerRepository;
            _technicianRepository = technicianRepository;
        }

        public IActionResult Index()
        {
            var repairs = _repairRepository.GetAll().OrderByDescending(repair => repair.DatumOtvaranja).ThenBy(repair => repair.Id);
            ViewData["Breadcrumbs"] = new[]
            {
                new BreadcrumbItemViewModel { Text = "Home", Url = Url.Action("Index", "Home") ?? "/" },
                new BreadcrumbItemViewModel { Text = "Repairs", Url = Url.Action(nameof(Index), "Repairs") ?? "/Repairs", IsActive = true }
            };
            return View(repairs);
        }

        public IActionResult Details(int id)
        {
            var repair = _repairRepository.GetById(id);
            if (repair == null)
            {
                return NotFound();
            }

            var model = new RepairDetailsViewModel
            {
                Repair = repair,
                Guitar = _guitarRepository.GetById((int)repair.Gitara.Id),
                Customer = _customerRepository.GetById((int)repair.Stranka.Id),
                Technician = _technicianRepository.GetById((int)repair.Tehnicar.Id)
            };

            ViewData["Breadcrumbs"] = new[]
            {
                new BreadcrumbItemViewModel { Text = "Home", Url = Url.Action("Index", "Home") ?? "/" },
                new BreadcrumbItemViewModel { Text = "Repairs", Url = Url.Action("Index", "Repairs") ?? "/Repairs" },
                new BreadcrumbItemViewModel { Text = "Repair #" + id, Url = Url.Action(nameof(Details), "Repairs", new { id }) ?? "/Repairs/Details/" + id, IsActive = true }
            };

            return View(model);
        }
    }
}