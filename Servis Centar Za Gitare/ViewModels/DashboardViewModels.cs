using System.Collections.Generic;
using Servis_Centar_Za_Gitare.models;

namespace Servis_Centar_Za_Gitare.ViewModels
{
    public class BreadcrumbItemViewModel
    {
        public string Text { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class HomeDashboardViewModel
    {
        public string OfficeName { get; set; } = string.Empty;
        public string OfficeAddress { get; set; } = string.Empty;
        public int TotalCustomers { get; set; }
        public int TotalGuitars { get; set; }
        public int TotalRepairs { get; set; }
        public int OpenRepairs { get; set; }
        public int TotalTechnicians { get; set; }
        public IEnumerable<Nalog> RecentRepairs { get; set; } = new List<Nalog>();
        public IEnumerable<Stranka> FeaturedCustomers { get; set; } = new List<Stranka>();
        public IEnumerable<ZapTehnicar> Technicians { get; set; } = new List<ZapTehnicar>();
    }

    public class CustomerDetailsViewModel
    {
        public Stranka Customer { get; set; } = new Stranka();
        public IEnumerable<Gitara> Guitars { get; set; } = new List<Gitara>();
        public IEnumerable<Nalog> Repairs { get; set; } = new List<Nalog>();
    }

    public class GuitarDetailsViewModel
    {
        public Gitara Guitar { get; set; } = new Gitara();
        public Stranka? Customer { get; set; }
        public Nalog? Repair { get; set; }
    }

    public class RepairDetailsViewModel
    {
        public Nalog Repair { get; set; } = new Nalog();
        public Gitara? Guitar { get; set; }
        public Stranka? Customer { get; set; }
        public ZapTehnicar? Technician { get; set; }
    }

    public class TechnicianDetailsViewModel
    {
        public ZapTehnicar Technician { get; set; } = new ZapTehnicar();
        public IEnumerable<Nalog> Repairs { get; set; } = new List<Nalog>();
    }
}