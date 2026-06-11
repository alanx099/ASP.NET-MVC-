using Microsoft.AspNetCore.Identity;

namespace Servis_Centar_Za_Gitare.models
{
    public class AppUser : IdentityUser
    {
        public virtual Stranka? Stranka { get; set; }
    }
}
