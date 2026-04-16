using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Servis_Centar_Za_Gitare.Data.Interfaces;
using Servis_Centar_Za_Gitare.ViewModels;

namespace Servis_Centar_Za_Gitare.Controllers
{
    public class GuitarsController : Controller
    {
        private readonly IGuitarRepository _guitarRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IRepairRepository _repairRepository;

        public GuitarsController(
            IGuitarRepository guitarRepository,
            ICustomerRepository customerRepository,
            IRepairRepository repairRepository)
        {
            _guitarRepository = guitarRepository;
            _customerRepository = customerRepository;
            _repairRepository = repairRepository;
        }

        public IActionResult Index()
        {
            var guitars = _guitarRepository.GetAll().OrderBy(guitar => guitar.Marka).ThenBy(guitar => guitar.SerijskiBroj);
            ViewData["Breadcrumbs"] = new[]
            {
                new BreadcrumbItemViewModel { Text = "Home", Url = Url.Action("Index", "Home") ?? "/" },
                new BreadcrumbItemViewModel { Text = "Guitars", Url = Url.Action(nameof(Index), "Guitars") ?? "/Guitars", IsActive = true }
            };
            return View(guitars);
        }

        public IActionResult Details(int id)
        {
            var guitar = _guitarRepository.GetById(id);
            if (guitar == null)
            {
                return NotFound();
            }

            var model = new GuitarDetailsViewModel
            {
                Guitar = guitar,
                Customer = _customerRepository.GetById((int)guitar.KupacId),
                Repair = _repairRepository.GetAll().FirstOrDefault(repair => repair.Gitara.Id == id)
            };

            ViewData["Breadcrumbs"] = new[]
            {
                new BreadcrumbItemViewModel { Text = "Home", Url = Url.Action("Index", "Home") ?? "/" },
                new BreadcrumbItemViewModel { Text = "Guitars", Url = Url.Action("Index", "Guitars") ?? "/Guitars" },
                new BreadcrumbItemViewModel { Text = guitar.SerijskiBroj, Url = Url.Action(nameof(Details), "Guitars", new { id }) ?? "/Guitars/Details/" + id, IsActive = true }
            };

            return View(model);
        }
    }
}