using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Servis_Centar_Za_Gitare.Data.Interfaces;
using Servis_Centar_Za_Gitare.models;

namespace Servis_Centar_Za_Gitare.Data.Repositories
{
    public class TechnicianRepository : ITechnicianRepository
    {
        private readonly AppDbContext _context;

        public TechnicianRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<ZapTehnicar> GetAll()
        {
            return _context.Tehnicari
                .Include(t => t.Znanja)
                .AsNoTracking()
                .ToList();
        }

        public ZapTehnicar? GetById(int id)
        {
            return _context.Tehnicari
                .Include(t => t.Znanja)
                .AsNoTracking()
                .FirstOrDefault(t => t.Id == id);
        }
    }
}