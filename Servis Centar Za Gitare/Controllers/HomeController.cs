using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Servis_Centar_Za_Gitare.Data.Interfaces;
using Servis_Centar_Za_Gitare.Data.Mock;
using Servis_Centar_Za_Gitare.enums;
using Servis_Centar_Za_Gitare.ViewModels;

namespace Servis_Centar_Za_Gitare.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IGuitarRepository _guitarRepository;
        private readonly IRepairRepository _repairRepository;
        private readonly ITechnicianRepository _technicianRepository;

        public HomeController(
            ICustomerRepository customerRepository,
            IGuitarRepository guitarRepository,
            IRepairRepository repairRepository,
            ITechnicianRepository technicianRepository)
        {
            _customerRepository = customerRepository;
            _guitarRepository = guitarRepository;
            _repairRepository = repairRepository;
            _technicianRepository = technicianRepository;
        }

        public IActionResult Index()
        {
            var repairs = _repairRepository.GetAll().ToList();
            var openStatuses = new[]
            {
                StatusNalogaEnum.Zaprimljen,
                StatusNalogaEnum.UObradi,
                StatusNalogaEnum.CekaDijelove
            };

            var model = new HomeDashboardViewModel
            {
                OfficeName = GuitarServiceMockData.Office.Ime,
                OfficeAddress = GuitarServiceMockData.Office.Adresa,
                TotalCustomers = _customerRepository.GetAll().Count(),
                TotalGuitars = _guitarRepository.GetAll().Count(),
                TotalRepairs = repairs.Count,
                OpenRepairs = repairs.Count(repair => openStatuses.Contains(repair.Status)),
                TotalTechnicians = _technicianRepository.GetAll().Count(),
                RecentRepairs = repairs.OrderByDescending(repair => repair.DatumOtvaranja).Take(3),
                FeaturedCustomers = _customerRepository.GetAll().Take(3),
                Technicians = _technicianRepository.GetAll()
            };

            ViewData["Breadcrumbs"] = new[]
            {
                new BreadcrumbItemViewModel { Text = "Home", Url = Url.Action(nameof(Index), "Home") ?? "/", IsActive = true }
            };

            return View(model);
        }

        public IActionResult Error()
        {
            ViewData["Breadcrumbs"] = new[]
            {
                new BreadcrumbItemViewModel { Text = "Home", Url = Url.Action(nameof(Index), "Home") ?? "/" },
                new BreadcrumbItemViewModel { Text = "Error", Url = Url.Action(nameof(Error), "Home") ?? "/Home/Error", IsActive = true }
            };

            return View();
        }
    }
}