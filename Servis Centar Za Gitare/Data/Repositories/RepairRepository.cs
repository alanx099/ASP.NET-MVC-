using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Servis_Centar_Za_Gitare.Data.Interfaces;
using Servis_Centar_Za_Gitare.models;

namespace Servis_Centar_Za_Gitare.Data.Repositories
{
    public class RepairRepository : IRepairRepository
    {
        private readonly AppDbContext _context;

        public RepairRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Nalog> GetAll()
        {
            return _context.Nalozi
                .Include(n => n.Gitara)
                    .ThenInclude(g => g.Marka)
                .Include(n => n.Gitara)
                    .ThenInclude(g => g.TipGitare)
                .Include(n => n.Stranka)
                .Include(n => n.Tehnicar)
                .Include(n => n.StatusNaloga)
                .Include(n => n.VrstaPopravka)
                .AsNoTracking()
                .ToList();
        }

        public Nalog? GetById(int id)
        {
            return _context.Nalozi
                .Include(n => n.Gitara)
                    .ThenInclude(g => g.Marka)
                .Include(n => n.Gitara)
                    .ThenInclude(g => g.TipGitare)
                .Include(n => n.Stranka)
                .Include(n => n.Tehnicar)
                .Include(n => n.StatusNaloga)
                .Include(n => n.VrstaPopravka)
                .AsNoTracking()
                .FirstOrDefault(n => n.Id == id);
        }
    }
}