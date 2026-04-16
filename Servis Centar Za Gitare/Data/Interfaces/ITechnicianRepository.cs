using System.Collections.Generic;
using Servis_Centar_Za_Gitare.models;

namespace Servis_Centar_Za_Gitare.Data.Interfaces
{
    public interface ITechnicianRepository
    {
        IEnumerable<ZapTehnicar> GetAll();
        ZapTehnicar? GetById(int id);
    }
}