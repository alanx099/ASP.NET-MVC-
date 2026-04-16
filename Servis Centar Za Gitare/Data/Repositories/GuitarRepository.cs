using System.Collections.Generic;
using Servis_Centar_Za_Gitare.Data.Interfaces;
using Servis_Centar_Za_Gitare.Data.Mock;
using Servis_Centar_Za_Gitare.models;

namespace Servis_Centar_Za_Gitare.Data.Repositories
{
    public class GuitarRepository : IGuitarRepository
    {
        public IEnumerable<Gitara> GetAll()
        {
            return GuitarServiceMockData.Guitars;
        }

        public Gitara? GetById(int id)
        {
            return GuitarServiceMockData.FindGuitar(id);
        }
    }
}