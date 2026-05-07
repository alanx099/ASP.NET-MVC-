using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Servis_Centar_Za_Gitare.Data.Interfaces;
using Servis_Centar_Za_Gitare.models;

namespace Servis_Centar_Za_Gitare.Data.Repositories
{
    public class GuitarRepository : IGuitarRepository
    {
        private readonly AppDbContext _context;

        public GuitarRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Gitara> GetAll()
        {
            return _context.Gitare
                .Include(g => g.Kupac)
                .AsNoTracking()
                .ToList();
        }

        public Gitara? GetById(int id)
        {
            return _context.Gitare
                .Include(g => g.Kupac)
                .AsNoTracking()
                .FirstOrDefault(g => g.Id == id);
        }
    }
}