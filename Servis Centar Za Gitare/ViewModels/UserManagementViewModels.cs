using Microsoft.AspNetCore.Mvc.Rendering;

namespace Servis_Centar_Za_Gitare.ViewModels
{
    public class UserManagementIndexViewModel
    {
        public IEnumerable<UserManagementUserViewModel> Users { get; set; } = Array.Empty<UserManagementUserViewModel>();
    }

    public class UserManagementUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
        public string? CustomerName { get; set; }
        public bool LockedOut { get; set; }
    }

    public class UserManagementEditViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string[] SelectedRoles { get; set; } = Array.Empty<string>();
        public long? StrankaId { get; set; }
        public bool LockedOut { get; set; }
        public IEnumerable<SelectListItem> RoleOptions { get; set; } = Array.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> CustomerOptions { get; set; } = Array.Empty<SelectListItem>();
    }
}
