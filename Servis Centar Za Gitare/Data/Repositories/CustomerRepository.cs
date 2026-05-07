using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Servis_Centar_Za_Gitare.Data.Interfaces;
using Servis_Centar_Za_Gitare.models;

namespace Servis_Centar_Za_Gitare.Data.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _context;

        public CustomerRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Stranka> GetAll()
        {
            return _context.Stranke
                .Include(s => s.Gitare)
                .AsNoTracking()
                .ToList();
        }

        public Stranka? GetById(int id)
        {
            return _context.Stranke
                .Include(s => s.Gitare)
                .AsNoTracking()
                .FirstOrDefault(s => s.Id == id);
        }
    }
}