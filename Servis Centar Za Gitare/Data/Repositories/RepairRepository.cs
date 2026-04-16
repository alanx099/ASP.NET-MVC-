using System.Collections.Generic;
using Servis_Centar_Za_Gitare.Data.Interfaces;
using Servis_Centar_Za_Gitare.Data.Mock;
using Servis_Centar_Za_Gitare.models;

namespace Servis_Centar_Za_Gitare.Data.Repositories
{
    public class RepairRepository : IRepairRepository
    {
        public IEnumerable<Nalog> GetAll()
        {
            return GuitarServiceMockData.Repairs;
        }

        public Nalog? GetById(int id)
        {
            return GuitarServiceMockData.FindRepair(id);
        }
    }
}