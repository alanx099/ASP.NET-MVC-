using Microsoft.EntityFrameworkCore;

namespace Servis_Centar_Za_Gitare.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {

        }
    }
}
