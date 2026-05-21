using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
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

    public class RepairQueueItemViewModel
    {
        public Nalog Repair { get; set; } = new Nalog();
        public bool NeedsTechnician { get; set; }
        public bool MissingSkillCoverage { get; set; }
    }

    public class TechnicianDetailsViewModel
    {
        public ZapTehnicar Technician { get; set; } = new ZapTehnicar();
        public IEnumerable<Nalog> Repairs { get; set; } = new List<Nalog>();
    }

    public class TechnicianFormViewModel
    {
        [Required]
        public ZapTehnicar Technician { get; set; } = new ZapTehnicar();
        public IEnumerable<SelectListItem> Poslovnice { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> TipGitareOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> VrstaPopravkeOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> KnowledgeOptions { get; set; } = new List<SelectListItem>();
        [MinLength(1, ErrorMessage = "Add at least one knowledge area.")]
        public string[] SelectedKnowledgePairs { get; set; } = Array.Empty<string>();
    }

    public class PendingSkillNeedViewModel
    {
        public int Count { get; set; }
        public int TipGitareId { get; set; }
        public int VrstaPopravkaId { get; set; }
        public string TipGitareName { get; set; } = string.Empty;
        public string VrstaPopravkaName { get; set; } = string.Empty;
    }

    public class ListStateViewModel
    {
        public string Sort { get; set; } = string.Empty;
        public string Direction { get; set; } = "asc";
        public int PageSize { get; set; } = 10;
        public int Take { get; set; } = 10;
        public int TotalCount { get; set; }
        public string ActionName { get; set; } = "Index";
        public IEnumerable<SelectListItem> SortOptions { get; set; } = new List<SelectListItem>();

        public bool IsAll => PageSize < 0;
        public bool IsDescending => Direction == "desc";
        public bool HasMore => !IsAll && Take < TotalCount;
        public int VisibleCount => Math.Min(Take, TotalCount);
        public int NextTake => IsAll ? TotalCount : Math.Min(Take + PageSize, TotalCount);
    }

    public class CustomerCreateViewModel
    {
        public Stranka Customer { get; set; } = new Stranka();
        public bool AddGuitar { get; set; }

        [StringLength(64, ErrorMessage = "Serial number can contain up to 64 characters.")]
        public string? GuitarSerijskiBroj { get; set; }

        public int? GuitarMarkaId { get; set; }

        [RegularExpression(@"^\d{1,2}$", ErrorMessage = "Enter a valid number of strings.")]
        [StringLength(4, ErrorMessage = "Number of strings can contain up to 4 characters.")]
        public string? GuitarBrojZica { get; set; }

        public int? GuitarTipGitareId { get; set; }

        public DateTime? GuitarDatumZaprimanja { get; set; }

        public IEnumerable<SelectListItem> Poslovnice { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Marke { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> TipoviGitare { get; set; } = new List<SelectListItem>();
    }
}
