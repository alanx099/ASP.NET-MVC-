using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Servis_Centar_Za_Gitare.Data.Interfaces;
using Servis_Centar_Za_Gitare.ViewModels;

namespace Servis_Centar_Za_Gitare.Controllers
{
    public class CustomersController : Controller
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IRepairRepository _repairRepository;

        public CustomersController(ICustomerRepository customerRepository, IRepairRepository repairRepository)
        {
            _customerRepository = customerRepository;
            _repairRepository = repairRepository;
        }

        public IActionResult Index()
        {
            var customers = _customerRepository.GetAll().OrderBy(customer => customer.Prezime).ThenBy(customer => customer.Ime);
            ViewData["Breadcrumbs"] = new[]
            {
                new BreadcrumbItemViewModel { Text = "Home", Url = Url.Action("Index", "Home") ?? "/" },
                new BreadcrumbItemViewModel { Text = "Customers", Url = Url.Action(nameof(Index), "Customers") ?? "/Customers", IsActive = true }
            };
            return View(customers);
        }

        public IActionResult Details(int id)
        {
            var customer = _customerRepository.GetById(id);
            if (customer == null)
            {
                return NotFound();
            }

            var model = new CustomerDetailsViewModel
            {
                Customer = customer,
                Guitars = customer.Gitare.OrderBy(guitar => guitar.SerijskiBroj),
                Repairs = _repairRepository.GetAll().Where(repair => repair.Stranka.Id == id).OrderByDescending(repair => repair.DatumOtvaranja)
            };

            ViewData["Breadcrumbs"] = new[]
            {
                new BreadcrumbItemViewModel { Text = "Home", Url = Url.Action("Index", "Home") ?? "/" },
                new BreadcrumbItemViewModel { Text = "Customers", Url = Url.Action("Index", "Customers") ?? "/Customers" },
                new BreadcrumbItemViewModel { Text = customer.Ime + " " + customer.Prezime, Url = Url.Action(nameof(Details), "Customers", new { id }) ?? "/Customers/Details/" + id, IsActive = true }
            };

            return View(model);
        }
    }
}