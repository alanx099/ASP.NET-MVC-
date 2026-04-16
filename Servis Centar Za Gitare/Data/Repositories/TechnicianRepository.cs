using System.Collections.Generic;
using Servis_Centar_Za_Gitare.Data.Interfaces;
using Servis_Centar_Za_Gitare.Data.Mock;
using Servis_Centar_Za_Gitare.models;

namespace Servis_Centar_Za_Gitare.Data.Repositories
{
    public class TechnicianRepository : ITechnicianRepository
    {
        public IEnumerable<ZapTehnicar> GetAll()
        {
            return GuitarServiceMockData.Technicians;
        }

        public ZapTehnicar? GetById(int id)
        {
            return GuitarServiceMockData.FindTechnician(id);
        }
    }
}