using System.Collections.Generic;
using Servis_Centar_Za_Gitare.models;

namespace Servis_Centar_Za_Gitare.Data.Interfaces
{
    public interface IRepairRepository
    {
        IEnumerable<Nalog> GetAll();
        Nalog? GetById(int id);
    }
}